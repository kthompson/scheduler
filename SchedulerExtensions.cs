using System;

namespace Scheduler
{
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Adds the specified timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="delay">The timeout.</param>
        /// <param name="action">The tick.</param>
        /// <returns></returns>
        public static IDisposable Schedule(this IScheduler @this, TimeSpan delay, Action<IScheduler> action)
        {
            return @this.Schedule(delay, action, null);
        }

        /// <summary>
        /// Adds the specified timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="delay">The timeout.</param>
        /// <param name="action">The tick.</param>
        /// <returns></returns>
        public static IDisposable Schedule(this IScheduler @this, TimeSpan delay, Action action)
        {
            return @this.Schedule(delay, s => action(), null);
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan interval, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler)
        {
            return @this.SchedulePeriodic(TimeSpan.Zero, interval, action, exceptionHandler);
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan interval, Action action)
        {
            return @this.SchedulePeriodic(TimeSpan.Zero, interval, s => action(), null);
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan interval, Action action, Action<Exception> exceptionHandler)
        {
            return @this.SchedulePeriodic(TimeSpan.Zero, interval, s => action(), (s, e) => exceptionHandler(e));
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan interval, Action<IScheduler> action)
        {
            return @this.SchedulePeriodic(TimeSpan.Zero, interval, action, null);
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan delay, TimeSpan interval, Action<IScheduler> action, Action<IScheduler, Exception> exceptionHandler)
        {
            return @this.SchedulePeriodic(delay, interval, action, exceptionHandler);
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan delay, TimeSpan interval, Action action)
        {
            return @this.SchedulePeriodic(delay, interval, s => action(), null);
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan delay, TimeSpan interval, Action action, Action<Exception> exceptionHandler)
        {
            return @this.SchedulePeriodic(delay, interval, s => action(), (s, e) => exceptionHandler(e));
        }

        /// <summary>
        /// Adds the specified repeating timeout action.
        /// </summary>
        /// <param name="this">The Dispatcher.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The tick.</param>
        /// <returns></returns>
        public static IDisposable SchedulePeriodic(this IScheduler @this, TimeSpan delay, TimeSpan interval, Action<IScheduler> action)
        {
            return @this.SchedulePeriodic(delay, interval, action, null);
        }
    }
}