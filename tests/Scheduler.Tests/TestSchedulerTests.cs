using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace Scheduler.Tests
{
    public class TestSchedulerTests
    {
        //[Fact]
        //public void Test1()
        //{
        //    var scheduler = new Scheduler();

        //    // In two seconds run SendNotification
        //    scheduler.Schedule(TimeSpan.FromSeconds(2), () => SendNotification());

        //    // In two seconds run SendNotification
        //    scheduler.Schedule(TimeSpan.FromSeconds(2), _ => SendNotification(), (scheduler1, exception) =>
        //    {
        //        // Handle Exception here
        //    });

        //    // Every thirty ms run UpdateStats
        //    scheduler.SchedulePeriodic(TimeSpan.FromMilliseconds(30), () => UpdateStats());
        //}


        [Fact]
        public void Schedule()
        {
            var scheduler = new TestScheduler();
            var x = 0;

            scheduler.Schedule(TimeSpan.FromSeconds(2), () => x++);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
            Assert.Equal(1, x);
        }

        [Fact]
        public void SchedulePeriodic()
        {
            var scheduler = new TestScheduler();
            var y = 0;

            scheduler.SchedulePeriodic(TimeSpan.FromSeconds(1), _ => y++);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
            Assert.Equal(3, y);
        }
    }
}
