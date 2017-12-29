using System;

namespace Scheduler
{
    /// <summary>
    /// Represents a disposable timeout object used internally by the TimeoutDispatcher
    /// </summary>
    internal class TimeoutObject : IDisposable
    {
        public TimeoutObject(Action<IScheduler> action, DateTimeOffset nextTick)
        {
            Action = action;
            NextTick = nextTick;
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
        /// Gets or sets the tick action.
        /// </summary>
        /// <value>
        /// The tick.
        /// </value>
        public Action<IScheduler> Action { get; }

        /// <summary>
        /// Gets or sets the next tick.
        /// </summary>
        /// <value>
        /// The next tick.
        /// </value>
        public DateTimeOffset NextTick { get; }
    }
}