using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

[Component("targetable")]
sealed class Targetable : Component, IListener<TileEntered>, IListener<TileLeft>
{
    protected override void OnAdded()
    {
        Events.Subscribe<TileEntered>(this);
        Events.Subscribe<TileLeft>(this);
    }

    public void HandleEvent(TileEntered e)
    {
        Owner.Game.TargetLayer.AddObjectToTile(Owner, e.Tile);
    }

    public void HandleEvent(TileLeft e)
    {
        Owner.Game.TargetLayer.RemoveObjectFromTile(Owner, e.Tile);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
