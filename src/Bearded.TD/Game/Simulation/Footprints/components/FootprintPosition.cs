using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints;

sealed class FootprintPosition : Component, IListener<FootprintChanged>
{
    private bool isActive;
    private PositionedFootprint? lastKnownFootprint;

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
        base.Activate();
        isActive = true;
        if (lastKnownFootprint != null)
        {
            calculatePosition(lastKnownFootprint.Value);
        }
    }

    public void HandleEvent(FootprintChanged @event)
    {
        lastKnownFootprint = @event.NewFootprint;
        if (isActive)
        {
            calculatePosition(@event.NewFootprint);
        }
    }

    private void calculatePosition(PositionedFootprint footprint)
    {
        var z = Owner.Game.Level.IsValid(footprint.RootTile)
            ? Owner.Game.GeometryLayer[footprint.RootTile].DrawInfo.Height
            : Unit.Zero;
        Owner.Position = footprint.CenterPosition.WithZ(z);
    }

    public override void Update(TimeSpan elapsedTime) {}
}
