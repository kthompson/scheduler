using System;

namespace Scheduler
{
    public interface IScheduler
    {
        /// <summary>
        /// Gets the scheduler's notion of current time.
        /// </summary>
        DateTimeOffset Now { get; }

        /// <summary>
        /// Adds the specified timeout action.
        /// </summary>
        /// <param name="delay">The timeout.</param>
        /// <param name="action">The tick.</param>
        /// <returns></returns>
        IDisposable Schedule(TimeSpan delay, Action<IScheduler> action);
    }
}