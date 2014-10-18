using System;

namespace Scheduler
{
    /// <summary>
    /// Represents a disposable timeout object used internally by the TimeoutDispatcher
    /// </summary>
    class TimeoutObject : IDisposable
    {

        public TimeoutObject(IScheduler scheduler, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler, TimeSpan delay, TimeSpan interval)
        {
            this.Scheduler = scheduler;
            this.Action = action;
            this.ExceptionHandler = exceptionHandler;
            this.NextTick = scheduler.Now.Add(delay);
            this.Interval = interval;
        }

        public TimeoutObject()
        {
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
}