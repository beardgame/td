using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;

namespace Bearded.TD.Game.Simulation.Weapons;

sealed partial class TargetEnemiesInRange
{
    public void HandleEvent(DrawComponents e)
    {
        tileRangeDrawer.Draw();

        debugDrawTargeting(e);
    }

    private void debugDrawTargeting(DrawComponents e)
    {
        if (UserSettings.Instance.Debug.TowerTargeting == false || weapon.RangeDrawStyle == TileRangeDrawer.RangeDrawStyle.DoNotDraw)
            return;

        e.Core.Primitives.DrawCircle(targetPosition.Position.NumericValue, 0.2f, 0.05f, Color.Red, 6);

        var emitPosition = (emitter?.EmitPosition ?? Owner.Position).NumericValue;
        var emitDirection = weapon.Direction.Vector.WithZ();
        var range = Parameters.Range.NumericValue;
        var coneAngle = Parameters.ConeOfFire ?? Angle.Zero;
        var coneMin = (weapon.Direction - coneAngle).Vector.WithZ();
        var coneMax = (weapon.Direction + coneAngle).Vector.WithZ();

        e.Core.Primitives.DrawLine(emitPosition, emitPosition + emitDirection * range, 0.05f, Color.Red);
        e.Core.Primitives.DrawLine(emitPosition, emitPosition + coneMin * range, 0.025f, Color.Red);
        e.Core.Primitives.DrawLine(emitPosition, emitPosition + coneMax * range, 0.025f, Color.Red);
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
