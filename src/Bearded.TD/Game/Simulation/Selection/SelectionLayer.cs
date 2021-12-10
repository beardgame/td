using System.Collections.Generic;
using Bearded.TD.Game.Meta;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Selection;

sealed class SelectionLayer
{
    private readonly MultiDictionary<Tile, ISelectable> selectablesByTile = new();

    public IEnumerable<ISelectable> SelectablesForTile(Tile tile) => selectablesByTile[tile];

    public void RegisterSelectable(Tile tile, ISelectable selectable) => selectablesByTile.Add(tile, selectable);

    public void UnregisterSelectable(Tile tile, ISelectable selectable) =>
        selectablesByTile.Remove(tile, selectable);
}