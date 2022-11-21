using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Weapons;

sealed partial class TargetEnemiesInRange
{
    public void HandleEvent(DrawComponents e)
    {
        tileRangeDrawer.Draw();
    }

    private IEnumerable<Tile> getTilesToDraw()
    {
        if (skipDrawThisFrame)
        {
            skipDrawThisFrame = false;
            return ImmutableHashSet<Tile>.Empty;
        }

        var allTiles = ImmutableHashSet.CreateRange(
            (Owner.Parent?.GetComponents<ITurret>() ?? Enumerable.Empty<ITurret>())
            .Select(t => t.Weapon)
            .SelectMany(w => w.GetComponents<IWeaponRangeDrawer>())
            .SelectMany(ranger => ranger.TakeOverDrawingThisFrame()));
        skipDrawThisFrame = false;
        return allTiles;
    }

    IEnumerable<Tile> IWeaponRangeDrawer.TakeOverDrawingThisFrame()
    {
        skipDrawThisFrame = true;
        recalculateTilesInRange();
        return tilesInRange;
    }
}
