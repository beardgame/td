using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Footprints;

sealed class TilePresence : Component, ITilePresence, IListener<TileEntered>, IListener<TileLeft>
{
    private readonly List<Tile> occupiedTiles = new();

    public event TileEventHandler? TileAdded;
    public event TileEventHandler? TileRemoved;

    public IEnumerable<Tile> OccupiedTiles => occupiedTiles.AsReadOnly();

    protected override void OnAdded()
    {
        Events.Subscribe<TileEntered>(this);
        Events.Subscribe<TileLeft>(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(TileEntered @event)
    {
        Argument.Satisfies(
            !occupiedTiles.Contains(@event.Tile),
            $"Attempted to add tile {@event.Tile} to tile presence but tile was already entered.");

        occupiedTiles.Add(@event.Tile);
        TileAdded?.Invoke(@event.Tile);
    }

    public void HandleEvent(TileLeft @event)
    {
        var removed = occupiedTiles.Remove(@event.Tile);
        TileRemoved?.Invoke(@event.Tile);

        Argument.Satisfies(
            removed, $"Attempted to remove tile {@event.Tile} from tile presence but tile was never entered.");
    }
}
