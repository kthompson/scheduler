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
        private readonly List<TimeoutObject> _timeoutObjects = new List<TimeoutObject>();

        public DateTimeOffset Now { get; private set; }

        public void AdvanceTo(DateTimeOffset time)
        {
            RunItems(time);
        }

        public void AdvanceBy(TimeSpan time)
        {
            AdvanceTo(this.Now.Add(time));
        }

        private void RunItems(DateTimeOffset target)
        {
            while (true)
            {
                // If we have no items left then exit the thread
                if (this._timeoutObjects.Count == 0)
                {
                    this.Now = target;
                    break;
                }

                // Get our next timeout object
                var nextTimeout = this._timeoutObjects.OrderBy(o => o.NextTick).First();
                if (nextTimeout.IsCancelled)
                {
                    this._timeoutObjects.Remove(nextTimeout);
                    continue;
                }

                // If our next timeout is not ready then break out
                var readyToRun = nextTimeout.NextTick <= target;
                if (!readyToRun)
                {
                    this.Now = target;
                    break;
                }

                // we're about to execute the action so just remove it now
                this._timeoutObjects.Remove(nextTimeout);

                this.Now = nextTimeout.NextTick;

                // Execute the tick and then figure out the next tick time
                try
                {
                    nextTimeout.Action(this);
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception, "TimeoutDispatcher");
                }
            }
        }

        public IDisposable Schedule(TimeSpan delay, Action<IScheduler> action)
        {
            var timeoutObj = new TimeoutObject(action, this.Now.Add(delay));

            this._timeoutObjects.Add(timeoutObj);

            return timeoutObj;
        }
    }
}
