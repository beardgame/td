using System;
using Bearded.TD.Utilities.Performance;
using Bearded.Utilities.SpaceTime;
using FluentAssertions;
using Xunit;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Tests.Utilities
{
    public sealed class ActivityTimerTests
    {
        private readonly ActivityTimer timer;
        private Instant time;

        private void advanceTime(double seconds)
        {
            time += new TimeSpan(seconds);
        }

        public ActivityTimerTests()
        {
            timer = new ActivityTimer(() => time);
        }

        [Fact]
        public void CanStartActivity()
        {
            timer.Start(Activity.UpdateGame);
        }

        [Fact]
        public void StoppingFailsIfNoActivityWasStarted()
        {
            Action stopping = () => timer.Stop(Activity.UpdateGame);

            stopping.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void StoppingSucceedsIfActivityWasStarted()
        {
            timer.Start(Activity.UpdateGame);

            Action stopping = () => timer.Stop(Activity.UpdateGame);

            stopping.Should().NotThrow();
        }

        [Fact]
        public void StoppingFailsIfDifferentActivityWasStarted()
        {
            timer.Start(Activity.UpdateGame);

            Action stopping = () => timer.Stop(Activity.RenderGame);

            stopping.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SecondStopFailsAfterSingleStart()
        {
            timer.Start(Activity.UpdateGame);
            timer.Stop(Activity.UpdateGame);

            Action stopping = () => timer.Stop(Activity.UpdateGame);

            stopping.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CanStopMultipleStartedActivitiesInReverseOrder()
        {
            timer.Start(Activity.UpdateGame);
            timer.Start(Activity.RenderGame);
            timer.Start(Activity.RenderGame);

            Action stoppingInReverseOrder = () =>
            {
                timer.Stop(Activity.RenderGame);
                timer.Stop(Activity.RenderGame);
                timer.Stop(Activity.UpdateGame);
            };

            stoppingInReverseOrder.Should().NotThrow();
        }

        [Fact]
        public void CanResetBeforeStarting()
        {
            Action resetting = () => timer.Reset();

            resetting.Should().NotThrow();
        }

        [Fact]
        public void CanResetAfterStarting()
        {
            timer.Start(Activity.UpdateGame);

            Action resetting = () => timer.Reset();

            resetting.Should().NotThrow();
        }

        [Fact]
        public void CanResetAfterStoppingAllStartedActivities()
        {
            timer.Start(Activity.UpdateGame);
            timer.Start(Activity.RenderGame);
            timer.Stop(Activity.RenderGame);
            timer.Stop(Activity.UpdateGame);

            Action resetting = () => timer.Reset();

            resetting.Should().NotThrow();
        }

        [Fact]
        public void CannotStopAfterResetting()
        {
            timer.Start(Activity.UpdateGame);
            timer.Reset();

            Action stopping = () => timer.Stop(Activity.UpdateGame);

            stopping.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ResettingBeforeStartingReturnsEmptyList()
        {
            var activities = timer.Reset();

            activities.Should().BeEmpty();
        }

        [Fact]
        public void ResettingReturnsAllStartedActivities()
        {
            timer.Start(Activity.UpdateGame);
            timer.Start(Activity.RenderGame);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(2).And
                .Contain(a => a.Activity == Activity.UpdateGame).And
                .Contain(a => a.Activity == Activity.RenderGame);
        }

        [Fact]
        public void ResettingReturnsAllStartedActivitiesEvenIfStopped()
        {
            timer.Start(Activity.UpdateGame);
            timer.Start(Activity.RenderGame);
            timer.Stop(Activity.RenderGame);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(2).And
                .Contain(a => a.Activity == Activity.UpdateGame).And
                .Contain(a => a.Activity == Activity.RenderGame);
        }

        [Fact]
        public void ResettingReturnsAllStartedActivitiesWithoutDuplicates()
        {
            timer.Start(Activity.UpdateGame);
            timer.Start(Activity.UpdateGame);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.Activity == Activity.UpdateGame);
        }

        [Fact]
        public void ResettingReturnsZeroTimespanWithInstantStartAndStop()
        {
            timer.Start(Activity.UpdateGame);
            timer.Stop(Activity.UpdateGame);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.TimeSpan == TimeSpan.Zero);
        }

        [Fact]
        public void ResettingReturnsZeroTimespanAfterStart()
        {
            timer.Start(Activity.UpdateGame);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.TimeSpan == TimeSpan.Zero);
        }

        [Fact]
        public void ResettingReturnsOneSecondTimespanOneSecondAfterStart()
        {
            timer.Start(Activity.UpdateGame);
            advanceTime(1);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.TimeSpan == TimeSpan.One);
        }

        [Fact]
        public void ResettingReturnsOneSecondTimespanOneSecondAfterStartAtNonZeroTime()
        {
            advanceTime(1);
            timer.Start(Activity.UpdateGame);
            advanceTime(1);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.TimeSpan == TimeSpan.One);
        }

        [Fact]
        public void ResettingReturnsTimeBetweenStartAndStopOfActivity()
        {
            advanceTime(1);
            timer.Start(Activity.UpdateGame);
            advanceTime(2);
            timer.Stop(Activity.UpdateGame);
            advanceTime(1);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.TimeSpan == new TimeSpan(2));
        }

        [Fact]
        public void ResettingReturnsTimeBetweenMultipleStartsAndStops()
        {
            advanceTime(1);
            timer.Start(Activity.UpdateGame);
            advanceTime(2);
            timer.Stop(Activity.UpdateGame);
            advanceTime(1);
            timer.Start(Activity.UpdateGame);
            advanceTime(3);
            timer.Stop(Activity.UpdateGame);
            advanceTime(10);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.TimeSpan == new TimeSpan(5));
        }

        [Fact]
        public void ResettingASecondTimeReturnsEmptyResult()
        {
            timer.Start(Activity.UpdateGame);
            advanceTime(1);
            timer.Reset();

            var activities = timer.Reset();

            activities.Should().BeEmpty();
        }

        [Fact]
        public void DoesNotCountTimeForFirstActivityWhileSecondActivityIsRunning()
        {
            timer.Start(Activity.UpdateGame);
            advanceTime(1);
            timer.Start(Activity.RenderGame);
            advanceTime(1);
            timer.Stop(Activity.RenderGame);
            advanceTime(1);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(2).And
                .Contain(a => a.Activity == Activity.UpdateGame && a.TimeSpan == new TimeSpan(2)).And
                .Contain(a => a.Activity == Activity.RenderGame && a.TimeSpan == new TimeSpan(1));
        }

        [Fact]
        public void ResettingReturnsCorrectResultForMultipleNestedActivities()
        {
            advanceTime(5);
            timer.Start(Activity.UpdateGame);
                advanceTime(2); // UpdateGame
                timer.Start(Activity.RenderGame);
                    advanceTime(5); // RenderGame
                timer.Stop(Activity.RenderGame);
                advanceTime(1); // UpdateGame
                timer.Start(Activity.RenderGame);
                    advanceTime(1);  // RenderGame
                    timer.Start(Activity.SwapBuffer);
                        advanceTime(1); // SwapBuffer
                        timer.Start(Activity.UpdateGame);
                            advanceTime(1); // UpdateGame
                        timer.Stop(Activity.UpdateGame);
                        advanceTime(1); // SwapBuffer
                    timer.Stop(Activity.SwapBuffer);
                    advanceTime(1);  // RenderGame

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(3).And
                .Contain(a => a.Activity == Activity.UpdateGame && a.TimeSpan == new TimeSpan(4)).And
                .Contain(a => a.Activity == Activity.RenderGame && a.TimeSpan == new TimeSpan(7)).And
                .Contain(a => a.Activity == Activity.SwapBuffer && a.TimeSpan == new TimeSpan(2));
        }

        [Fact]
        public void ResettingWithNewActivityStartsThatActivity()
        {
            timer.Start(Activity.UpdateGame);
            advanceTime(1);
            timer.Reset(Activity.UpdateGame);
            advanceTime(2);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.Activity == Activity.UpdateGame && a.TimeSpan == new TimeSpan(2));
        }

        [Fact]
        public void StartReturnsDisposableThatStopsActivityOnDispose()
        {
            var disposable = (IDisposable)timer.Start(Activity.UpdateGame);
            advanceTime(1);
            disposable.Dispose();
            advanceTime(1);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.Activity == Activity.UpdateGame && a.TimeSpan == new TimeSpan(1));
        }

        [Fact]
        public void StartReturnsDisposableThatStopsActivityWithUsingStatement()
        {
            using (timer.Start(Activity.UpdateGame))
            {
                advanceTime(1);
            }
            advanceTime(1);

            var activities = timer.Reset();

            activities.Should()
                .HaveCount(1).And
                .Contain(a => a.Activity == Activity.UpdateGame && a.TimeSpan == new TimeSpan(1));
        }
    }
}
