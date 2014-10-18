using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler
{
    public class TestScheduler : IScheduler
    {
        private readonly List<TimeoutObject> _timeoutObjects;
        private DateTimeOffset _now;

        public TestScheduler()
        {
            this._timeoutObjects = new List<TimeoutObject>();
        }

        public DateTimeOffset Now
        {
            get { return _now; }
            private set
            {
                _now = value;
                RunItems();
            }
        }

        public void AdvanceTo(DateTimeOffset time)
        {
            this.Now = time;
        }

        public void AdvanceBy(TimeSpan time)
        {
            this.Now = this.Now.Add(time);
        }

        private void RunItems()
        {
            while (true)
            {
                // If we have no items left then exit the thread
                if (this._timeoutObjects.Count == 0)
                    break;

                // Get our next timeout object
                var nextTimeout = this._timeoutObjects.OrderBy(o => o.NextTick).First();

                if (nextTimeout.IsCancelled)
                {
                    this._timeoutObjects.Remove(nextTimeout);
                    continue;
                }

                // If our next timeout is not ready then break out
                var timeRemaining = nextTimeout.TimeRemaining;
                if (timeRemaining.TotalMilliseconds > 0)
                    break;

                // If the object we're about to execute is a one time action then just remove it now
                if (nextTimeout.Interval == TimeSpan.Zero)
                {
                    this._timeoutObjects.Remove(nextTimeout);
                }

                // Execute the tick and then figure out the next tick time
                try
                {
                    nextTimeout.Action(this);
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception, "TimeoutDispatcher");

                    try
                    {
                        if (nextTimeout.ExceptionHandler != null)
                            nextTimeout.ExceptionHandler(this, exception);
                    }
                    catch (Exception)
                    {
                    }
                }

                nextTimeout.NextTick = nextTimeout.NextTick.Add(nextTimeout.Interval);
            }
        }

        public IDisposable Schedule(TimeSpan delay, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler)
        {
            return this.AddInternal(new TimeoutObject(this, action, exceptionHandler, delay, TimeSpan.Zero));
        }

        private IDisposable AddInternal(TimeoutObject timeoutObj)
        {
            this._timeoutObjects.Add(timeoutObj);
            return timeoutObj;
        }

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

    }
}
