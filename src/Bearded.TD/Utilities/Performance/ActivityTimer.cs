using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Utilities.Performance
{
    sealed class ActivityTimer
    {
        private readonly Func<Instant> getCurrentTime;

        private readonly Dictionary<Activity, TimeSpan> allActivities = new();
        private readonly Stack<(Activity Activity, Instant StartTime)> currentActivities = new();

        public ActivityTimer(Func<Instant> getTime)
        {
            getCurrentTime = getTime;
        }

        public void Start(Activity activity)
        {
            var currentTime = getCurrentTime();

            tryPausingCurrentActivity(currentTime);
            startNewActivity(activity, currentTime);
        }

        public void Stop(Activity activity)
        {
            if (currentActivities.Peek().Activity != activity)
                throw new InvalidOperationException(
                    "Can only stop running activity.");

            var currentTime = getCurrentTime();

            stopCurrentActivity(currentTime);
            tryResumingPreviousActivity(currentTime);
        }

        public ImmutableArray<TimedActivity> Reset(Activity? newActivity = null)
        {
            var currentTime = getCurrentTime();
            tryPausingCurrentActivity(currentTime);

            var result = prepareResultsFromMeasuredActivities();

            currentActivities.Clear();
            allActivities.Clear();

            if (newActivity is { } activity)
                startNewActivity(activity, currentTime);

            return result;
        }

        private void startNewActivity(Activity activity, Instant currentTime)
        {
            currentActivities.Push((activity, currentTime));
            allActivities.TryAdd(activity, TimeSpan.Zero);
        }

        private void tryPausingCurrentActivity(Instant currentTime)
        {
            if (currentActivities.Count == 0)
                return;

            var (previousActivity, previousStartTime) = currentActivities.Peek();
            addIntervalToActivity(previousActivity, previousStartTime, currentTime);
        }

        private void tryResumingPreviousActivity(Instant currentTime)
        {
            if (currentActivities.Count <= 0)
                return;

            var (previousActivity, _) = currentActivities.Pop();
            currentActivities.Push((previousActivity, currentTime));
        }

        private void stopCurrentActivity(Instant currentTime)
        {
            var (activity, startTime) = currentActivities.Pop();
            addIntervalToActivity(activity, startTime, currentTime);
        }

        private void addIntervalToActivity(Activity activity, Instant startTime, Instant endTime)
        {
            allActivities[activity] += endTime - startTime;
        }

        private ImmutableArray<TimedActivity> prepareResultsFromMeasuredActivities()
        {
            var builder = ImmutableArray.CreateBuilder<TimedActivity>(allActivities.Count);

            foreach (var (activity, time) in allActivities)
            {
                builder.Add(new TimedActivity(activity, time));
            }

            return builder.MoveToImmutable();
        }
    }
}
