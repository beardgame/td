using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints;

sealed class FootprintPosition : Component, IListener<FootprintChanged>
{
    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(FootprintChanged @event)
    {
        calculatePosition(@event.NewFootprint);
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
