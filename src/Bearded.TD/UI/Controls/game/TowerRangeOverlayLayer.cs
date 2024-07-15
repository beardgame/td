using System;
using System.Collections.Immutable;
using System.Linq;
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
        if (drawStyle == RangeDrawStyle.DoNotDraw)
        {
            return null;
        }

        var weaponRanges = gameObject.GetComponents<ITurret>()
            .SelectMany(turret => turret.Weapon.GetComponents<IWeaponRange>())
            .ToImmutableArray();
        if (weaponRanges.IsEmpty)
        {
            return null;
        }

        var overlay = new TowerRangeOverlayLayer(weaponRanges, drawStyle);
        return overlays.Activate(overlay);
    }

    private readonly ImmutableArray<IWeaponRange> weaponRanges;
    private readonly RangeDrawStyle drawStyle;

    public DrawOrder DrawOrder => DrawOrder.TowerRange;

    private TowerRangeOverlayLayer(ImmutableArray<IWeaponRange> weaponRanges, RangeDrawStyle drawStyle)
    {
        this.weaponRanges = weaponRanges;
        this.drawStyle = drawStyle;
    }

    public void Draw(IOverlayDrawer context)
    {
        var allTilesInRange = weaponRanges.SelectMany(r => r.GetTilesInRange());
        var area = Area.From(allTilesInRange);
        var brush = drawStyle switch
        {
            RangeDrawStyle.DrawFull => OverlayBrush.TowerRangeFull,
            RangeDrawStyle.DrawMinimally => OverlayBrush.TowerRangeMinimal,
            _ => throw new ArgumentOutOfRangeException(nameof(drawStyle)),
        };
        context.Draw(area, brush);
    }
}
