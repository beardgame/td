using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Footprints;

[Component("footprintTransformation")]
sealed class FootprintTransformation : Component, ITransformable, IListener<FootprintChanged>
{
    public Matrix2 LocalCoordinateTransform { get; private set; }
    public Angle LocalOrientationTransform { get; private set; }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(FootprintChanged @event)
    {
        LocalOrientationTransform = @event.NewFootprint.Orientation;
        LocalCoordinateTransform = LocalOrientationTransform.Transformation;
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
