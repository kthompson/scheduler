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
        /// <param name="exceptionHandler">the exception handler </param>
        /// <returns></returns>
        IDisposable Schedule(TimeSpan delay, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler);

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <param name="exceptionHandler">The exception handler </param>
        /// <returns></returns>
        IDisposable SchedulePeriodic(TimeSpan initialDelay, TimeSpan interval, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler);
    }
}