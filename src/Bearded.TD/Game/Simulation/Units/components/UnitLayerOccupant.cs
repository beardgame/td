using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

sealed class UnitLayerOccupant : Component, IListener<TileEntered>, IListener<TileLeft>
{
    protected override void OnAdded()
    {
        Events.Subscribe<TileEntered>(this);
        Events.Subscribe<TileLeft>(this);
    }

    public void HandleEvent(TileEntered e)
    {
        Owner.Game.UnitLayer.AddEnemyToTile(e.Tile, Owner);
    }

    public void HandleEvent(TileLeft e)
    {
        Owner.Game.UnitLayer.RemoveEnemyFromTile(e.Tile, Owner);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

