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
            this.TimeoutObjects = new List<TimeoutObject>();
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
                NextTick = this.Now.Add(delay),
                Interval = TimeSpan.Zero
            });
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <param name="exceptionHandler">The exception handler </param>
        /// <returns></returns>
        public IDisposable SchedulePeriodic(TimeSpan initialDelay, TimeSpan interval, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (initialDelay <= TimeSpan.Zero)
                initialDelay = interval;

            return this.AddInternal(new TimeoutObject
            {
                Scheduler = this,
                Action = action,
                ExceptionHandler = exceptionHandler,
                NextTick = Now.Add(initialDelay),
                Interval = interval
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
            lock (this.TimeoutObjects)
            {
                this.TimeoutObjects.Add(timeoutObj);
                if (this.TimeoutObjects.Count == 1)
                    this.StartTimerThread();

                Monitor.PulseAll(this.TimeoutObjects);
            }

            return timeoutObj;
        }

        /// <summary>
        /// Removes the action and stops the thread if necessary.
        /// </summary>
        /// <param name="timeoutObj">The timeout obj.</param>
        private void RemoveInternal(TimeoutObject timeoutObj)
        {
            lock (this.TimeoutObjects)
            {
                this.TimeoutObjects.Remove(timeoutObj);
                if (this.TimeoutObjects.Count == 0)
                    this.StopTimerThread();

                Monitor.PulseAll(this.TimeoutObjects);
            }
        }

        /// <summary>
        /// Clears the actions and stops the thread.
        /// </summary>
        private void ClearInternal()
        {
            lock (this.TimeoutObjects)
            {
                this.TimeoutObjects.Clear();
                this.StopTimerThread();
                Monitor.PulseAll(this.TimeoutObjects);
            }
        }

        #endregion

        #region Private Methods

        private void StartTimerThread()
        {
            lock (this.TimeoutObjects)
            {
                this.TimerThread = new Thread(this.RunTimerThread);
                this.TimerThread.IsBackground = true;
                this.TimerThread.Start();
            }
        }

        private void StopTimerThread()
        {
            lock (this.TimeoutObjects)
            {
                // Set the timerthread to null which is the key to stop it and then signal it
                this.TimerThread = null;
                Monitor.PulseAll(this.TimeoutObjects);
            }
        }

        private void RunTimerThread()
        {
            var thread = Thread.CurrentThread;

            while (true)
            {
                TimeoutObject nextTimeout = null;

                lock (this.TimeoutObjects)
                {
                    // The thread is no longer running so break out
                    if (this.TimerThread != thread)
                        break;

                    // If we have no items left then exit the thread
                    if (this.TimeoutObjects.Count == 0)
                        break;

                    // Get our next timeout object
                    nextTimeout = this.TimeoutObjects.Where(o => !o.IsExecuting).OrderBy(o => o.NextTick).FirstOrDefault();

                    // If we didn't find anything that means all timeouts are executing still
                    // So just wait indefinitely until someone wakes us up
                    if (nextTimeout == null)
                    {
                        Monitor.Wait(this.TimeoutObjects);
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
                        Monitor.Wait(this.TimeoutObjects, timeRemaining);
                        continue;
                    }

                    // If the object we're about to execute is a one time action then just remove it now
                    if (nextTimeout.Interval == TimeSpan.Zero)
                        this.RemoveInternal(nextTimeout);
                }

                // Trigger the next timeout
                // Create a local variable to avoid issues with the closure
                var obj = nextTimeout;
                obj.IsExecuting = true;
                ThreadPool.UnsafeQueueUserWorkItem(data =>
                {
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

                    obj.NextTick = Now.Add(obj.Interval);
                    obj.IsExecuting = false;

                    // If this is a repeating job then we should probably notify our thread
                    // That we updated the next tick time so it knows how long to wait
                    if (obj.Interval == TimeSpan.Zero)
                        return;

                    lock (this.TimeoutObjects)
                    {
                        // Since some time may have passed make sure we're still valid before signaling
                        if (this.TimeoutObjects.Contains(obj))
                            Monitor.PulseAll(this.TimeoutObjects);
                    }
                }, null);
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

        protected Thread TimerThread
        {
            get;
            private set;
        }

        protected List<TimeoutObject> TimeoutObjects
        {
            get;
            private set;
        }

        #endregion

        #region Classes

        /// <summary>
        /// Represents a disposable timeout object used internally by the TimeoutDispatcher
        /// </summary>
        protected class TimeoutObject : IDisposable
        {
            public TimeoutObject()
            {
            }

            public TimeoutObject(IScheduler scheduler, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler, TimeSpan delay, TimeSpan interval)
            {
                this.Scheduler = scheduler;
                this.Action = action;
                this.ExceptionHandler = exceptionHandler;
                this.NextTick = scheduler.Now.Add(delay);
                this.Interval = interval;
            }

            public void Dispose()
            {
                this.IsCancelled = true;
            }

            /// <summary>
            /// Gets a value indicating whether this instance is cancelled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
            /// </value>
            public bool IsCancelled { get; private set; }

            /// <summary>
            /// Gets or sets the dispatcher.
            /// </summary>
            /// <value>
            /// The dispatcher.
            /// </value>
            public IScheduler Scheduler
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the tick action.
            /// </summary>
            /// <value>
            /// The tick.
            /// </value>
            public Action<IScheduler> Action
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the exception action.
            /// </summary>
            /// <value>
            /// The exception handler.
            /// </value>
            public Action<IScheduler, Exception> ExceptionHandler
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is executing.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is executing; otherwise, <c>false</c>.
            /// </value>
            public bool IsExecuting
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the next tick.
            /// </summary>
            /// <value>
            /// The next tick.
            /// </value>
            public DateTimeOffset NextTick
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the interval.
            /// </summary>
            /// <value>
            /// The interval.
            /// </value>
            public TimeSpan Interval
            {
                get;
                set;
            }

            /// <summary>
            /// Gets a value indicating whether the NextTick has elapsed.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the NextTick has elapsed; otherwise, <c>false</c>.
            /// </value>
            public bool IsElapsed
            {
                get { return (Scheduler.Now >= this.NextTick); }
            }

            /// <summary>
            /// Gets the time remaining.
            /// </summary>
            public TimeSpan TimeRemaining
            {
                get { return this.NextTick.Subtract(Scheduler.Now); }
            }

        }

        #endregion

    }
}