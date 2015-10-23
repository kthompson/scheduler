using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Scheduler
{
    /// <summary>
    /// Class used for scheduling actions and repeatable actions
    /// </summary>
    public sealed class Scheduler : IScheduler
    {
        /// <summary>
        /// Gets the scheduler's notion of current time.
        /// </summary>
        public DateTimeOffset Now
        {
            get
            {
                return DateTimeOffset.UtcNow;
            }
        }

        #region Constructors

        public Scheduler()
        {
            this._timeoutObjects = new List<TimeoutObject>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified timeout action.
        /// </summary>
        /// <param name="delay">The timeout.</param>
        /// <param name="action">The tick.</param>
        /// <param name="exceptionHandler">the exception handler </param>
        /// <returns></returns>
        public IDisposable Schedule(TimeSpan delay, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            return this.AddInternal(new TimeoutObject
            {
                Scheduler = this,
                Action = action,
                ExceptionHandler = exceptionHandler,
                NextTick = this.Now.Add(delay)
            });
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            this.ClearInternal();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Adds the action and starts the thread if necessary.
        /// </summary>
        /// <param name="timeoutObj">The timeout obj.</param>
        private IDisposable AddInternal(TimeoutObject timeoutObj)
        {
            lock (this._timeoutObjects)
            {
                this._timeoutObjects.Add(timeoutObj);
                if (this._timeoutObjects.Count == 1)
                    this.StartTimerThread();

                Monitor.PulseAll(this._timeoutObjects);
            }

            return timeoutObj;
        }

        /// <summary>
        /// Removes the action and stops the thread if necessary.
        /// </summary>
        /// <param name="timeoutObj">The timeout obj.</param>
        private void RemoveInternal(TimeoutObject timeoutObj)
        {
            lock (this._timeoutObjects)
            {
                this._timeoutObjects.Remove(timeoutObj);
                if (this._timeoutObjects.Count == 0)
                    this.StopTimerThread();

                Monitor.PulseAll(this._timeoutObjects);
            }
        }

        /// <summary>
        /// Clears the actions and stops the thread.
        /// </summary>
        private void ClearInternal()
        {
            lock (this._timeoutObjects)
            {
                this._timeoutObjects.Clear();
                this.StopTimerThread();
                Monitor.PulseAll(this._timeoutObjects);
            }
        }

        #endregion

        #region Private Methods

        private void StartTimerThread()
        {
            lock (this._timeoutObjects)
            {
                this._timerThread = new Thread(this.RunTimerThread);
                this._timerThread.IsBackground = true;
                this._timerThread.Start();
            }
        }

        private void StopTimerThread()
        {
            lock (this._timeoutObjects)
            {
                // Set the timerthread to null which is the key to stop it and then signal it
                this._timerThread = null;
                Monitor.PulseAll(this._timeoutObjects);
            }
        }

        private void RunTimerThread()
        {
            var thread = Thread.CurrentThread;

            while (true)
            {
                TimeoutObject nextTimeout;

                lock (this._timeoutObjects)
                {
                    // The thread is no longer running so break out
                    if (this._timerThread != thread)
                        break;

                    // If we have no items left then exit the thread
                    if (this._timeoutObjects.Count == 0)
                        break;

                    // Get our next timeout object
                    nextTimeout = this._timeoutObjects.OrderBy(o => o.NextTick).FirstOrDefault();

                    // If we didn't find anything that means all timeouts are executing still
                    // So just wait indefinitely until someone wakes us up
                    if (nextTimeout == null)
                    {
                        Monitor.Wait(this._timeoutObjects);
                        continue;
                    }

                    if (nextTimeout.IsCancelled)
                    {
                        this.RemoveInternal(nextTimeout);
                        continue;
                    }

                    // If our next timeout is not ready then wait for it to be
                    var timeRemaining = nextTimeout.TimeRemaining;
                    if (timeRemaining.TotalMilliseconds > 0)
                    {
                        // Wait until we get pulsed or our timeout is up then make sure we're still running and try again
                        Monitor.Wait(this._timeoutObjects, timeRemaining);
                        continue;
                    }

                    // we're about to execute this action so just remove it now
                    this.RemoveInternal(nextTimeout);
                }

                // Trigger the next timeout
                ThreadPool.UnsafeQueueUserWorkItem(HandleTimeout, nextTimeout);
            }
        }

        private void HandleTimeout(object data)
        {
            var obj = (TimeoutObject) data;

            // Execute the tick and then figure out the next tick time
            try
            {
                obj.Action(this);
            }
            catch (Exception exception)
            {
                Break();
                Trace.WriteLine(exception, "TimeoutDispatcher");

                try
                {
                    if (obj.ExceptionHandler != null)
                        obj.ExceptionHandler(this, exception);
                }
                catch (Exception)
                {
                    //if there was an exception in the exception handler we will just silence it.
                    Break();
                }
            }
        }

        [DebuggerHidden]
        private static void Break()
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        #endregion

        #region Properties

        private Thread _timerThread;

        private readonly List<TimeoutObject> _timeoutObjects;

        #endregion
    }
}