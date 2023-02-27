using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

[Component("tilePresence")]
sealed class TilePresence : Component, IListener<TileEntered>, IListener<TileLeft>
{
    protected override void OnAdded()
    {
        Events.Subscribe<TileEntered>(this);
        Events.Subscribe<TileLeft>(this);
    }

    public void HandleEvent(TileEntered e)
    {
        Owner.Game.ObjectLayer.AddObjectToTile(Owner, e.Tile);
    }

    public void HandleEvent(TileLeft e)
    {
        Owner.Game.ObjectLayer.RemoveObjectFromTile(Owner, e.Tile);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
