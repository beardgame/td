using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Meta;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Performance;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls;

sealed class PerformanceOverlay : UpdateableNavigationNode<Void>
{
    private IEnumerable<ImmutableArray<TimedActivity>> activityFrames = null!;

    public int FramesConsideredForAverage => 20;
    public ImmutableArray<(string Name, TimeSpan Time)> LastFrame { get; private set; }

    public event VoidEventHandler? LastFrameUpdated;

    protected override void Initialize(DependencyResolver dependencies, Void parameters)
    {
        base.Initialize(dependencies, parameters);

        activityFrames = dependencies.Resolve<IEnumerable<ImmutableArray<TimedActivity>>>();
    }

    public override void Update(UpdateEventArgs args)
    {
        if (!UserSettings.Instance.Debug.PerformanceOverlay)
        {
            Terminate();
            Navigation.Close(this);
            return;
        }

        var allActivityAverages = activityFrames
            .TakeLast(FramesConsideredForAverage)
            .SelectMany(a => a)
            .GroupBy(a => a.Activity)
            .Select(g => (
                Activity: g.Key,
                Time: g.Average(a => a.TimeSpan.NumericValue).S()
            ))
            .OrderBy(a => a.Activity)
            .ToList();

        LastFrame = allActivityAverages
            .Select(a => (a.Activity.ToString(), a.Time))
            .Prepend(("Total", allActivityAverages.Sum(a => a.Time.NumericValue).S()))
            .ToImmutableArray();

        LastFrameUpdated?.Invoke();
    }


}