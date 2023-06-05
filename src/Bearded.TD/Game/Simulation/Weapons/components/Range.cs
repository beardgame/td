using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("range")]
sealed class Range :
    Component<Range.IParameters>,
    IWeaponRange,
    IWeaponRangeDrawer,
    IListener<DrawComponents>
{
    public enum Type
    {
        LineOfSight = 0,
        FloodFill = 1,

        Default = LineOfSight,
    }

    private readonly IRanger ranger;
    private IWeaponState weapon = null!;
    private PassabilityLayer passabilityLayer = null!;
    private TileRangeDrawer tileRangeDrawer = null!;

    private Unit currentRange;
    private Angle? currentMaxTurningAngle;
    private Instant nextTileInRangeRecalculationTime;
    private ImmutableArray<Tile> tilesInRange = ImmutableArray<Tile>.Empty;

    Unit IWeaponRange.Range => currentRange;
    private bool skipDrawThisFrame;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }
        Unit MinimumRange { get; }
        [Modifiable(1)]
        TimeSpan ReCalculateTilesInRangeInterval { get; }

        Type Type { get; }
    }

    public Range(IParameters parameters) : base(parameters)
    {
        ranger = parameters.Type switch
        {
            Type.LineOfSight => new LineOfSightRanger(),
            Type.FloodFill => new FloodFillRanger(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => weapon = c);
    }

    public override void Activate()
    {
        passabilityLayer = Owner.Game.PassabilityObserver.GetLayer(Passability.Projectile);

        tileRangeDrawer = new TileRangeDrawer(
            Owner.Game, () => weapon.RangeDrawStyle, getTilesToDraw, Color.Green);

        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public ImmutableArray<Tile> GetTilesInRange()
    {
        ensureTilesInRangeUpToDate();
        return tilesInRange;
    }

    private void ensureTilesInRangeUpToDate()
    {
        var rangeChanged = currentRange != Parameters.Range
            || !currentMaxTurningAngle.Equals(weapon.MaximumTurningAngle);

        if (rangeChanged || Owner.Game.Time >= nextTileInRangeRecalculationTime)
        {
            recalculateTilesInRange();
        }
    }

    private void recalculateTilesInRange()
    {
        currentMaxTurningAngle = weapon.MaximumTurningAngle;
        currentRange = Parameters.Range;

        tilesInRange = ranger.GetTilesInRange(
            Owner.Game, passabilityLayer, weapon, Parameters.MinimumRange, Parameters.Range
            );

        nextTileInRangeRecalculationTime = Owner.Game.Time + Parameters.ReCalculateTilesInRangeInterval;
    }

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
            .SelectMany(r => r.TakeOverDrawingThisFrame()));
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

