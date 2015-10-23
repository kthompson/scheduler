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
                var timeRemaining = nextTimeout.NextTick.Subtract(this.Now);
                if (timeRemaining.TotalMilliseconds > 0)
                    break;

                // we're about to execute the action so just remove it now
                this._timeoutObjects.Remove(nextTimeout);

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
            }
        }

        public IDisposable Schedule(TimeSpan delay, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler)
        {
            var timeoutObj = new TimeoutObject
            {
                Action = action,
                ExceptionHandler = exceptionHandler,
                NextTick = this.Now.Add(delay),
            };

            this._timeoutObjects.Add(timeoutObj);

            return timeoutObj;
        }
    }
}
