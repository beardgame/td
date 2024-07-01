using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Tiles;

namespace Bearded.TD.UI.Controls;

sealed class TowerRangeOverlayLayer : IOverlayLayer
{
    public static IActiveOverlay? CreateAndActivateForGameObject(
        ActiveOverlays overlays, GameObject gameObject, RangeDrawStyle drawStyle)
    {
        if (!gameObject.GetComponents<ITurret>().Any() || drawStyle == RangeDrawStyle.DoNotDraw)
        {
            return null;
        }

        var weaponRanges = gameObject.GetComponents<ITurret>()
            .SelectMany(turret => turret.Weapon.GetComponents<IWeaponRange>())
            .ToImmutableArray();
        var reader = new TowerRangeReader(weaponRanges);

        var overlay = new TowerRangeOverlayLayer(reader, drawStyle);
        return overlays.Activate(overlay);
    }

    private readonly TowerRangeReader reader;
    private readonly RangeDrawStyle drawStyle;

    public DrawOrder DrawOrder => DrawOrder.TowerRange;

    private TowerRangeOverlayLayer(TowerRangeReader reader, RangeDrawStyle drawStyle)
    {
        this.reader = reader;
        this.drawStyle = drawStyle;
    }

    public void Draw(IOverlayDrawer context)
    {
        context.Area(
            Color.Green * (drawStyle == RangeDrawStyle.DrawFull ? 0.5f : 0.25f),
            reader.GetAreaInRange());
    }

    private sealed class TowerRangeReader(ImmutableArray<IWeaponRange> weaponRanges)
    {
        public IArea GetAreaInRange()
        {
            var allTiles = weaponRanges.SelectMany(r => r.GetTilesInRange());
            return Area.From(allTiles);
        }
    }
}
