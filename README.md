# Scheduler

 [![Nuget](https://img.shields.io/nuget/v/SharpScheduler.svg)](https://www.nuget.org/packages/SharpScheduler/)

Scheduler is a simple C# implementation of a scheduler that support recurring events and unit testing with "Time travel".

	nuget install SharpScheduler   # Install Scheduler


# Example

```csharp

    IScheduler scheduler = new Scheduler();

    // In two seconds run SendNotification
    scheduler.Schedule(TimeSpan.FromSeconds(2), () => SendNotification());

    // In two seconds run SendNotification
    scheduler.Schedule(TimeSpan.FromSeconds(2), _ => SendNotification(), exception =>
    {
        // Handle Exception here
    });

    // Every thirty ms run UpdateStats
    scheduler.SchedulePeriodic(TimeSpan.FromMilliseconds(30), () => UpdateStats());
```

# Test Example

```csharp

    var scheduler = new TestScheduler();
    var x = 0;

    scheduler.Schedule(TimeSpan.FromSeconds(2), () => x++);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
    Assert.Equal(1, x);
```

```csharp

    var scheduler = new TestScheduler();
    var y = 0;

    scheduler.SchedulePeriodic(TimeSpan.FromSeconds(1), _ => y++);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
    Assert.Equal(3, y);
```
