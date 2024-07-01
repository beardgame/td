using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("range")]
sealed class Range :
    Component<Range.IParameters>,
    IWeaponRange
{
    public enum Type
    {
        LineOfSight = 0,
        FloodFill = 1,

        Default = LineOfSight,
    }

    private IRanger ranger = null!;
    private IWeaponState weapon = null!;
    private PassabilityLayer passabilityLayer = null!;

    private Unit currentRange;
    private Angle? currentMaxTurningAngle;
    private Tile currentTile;
    private Instant nextTileInRangeRecalculationTime;
    private ImmutableArray<Tile> tilesInRange = ImmutableArray<Tile>.Empty;

    Unit IWeaponRange.Range => currentRange;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }
        Unit MinimumRange { get; }
        [Modifiable(1)]
        TimeSpan ReCalculateTilesInRangeInterval { get; }

        Type Type { get; }
    }

    public Range(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c =>
        {
            weapon = c;
            ranger = Parameters.Type switch
            {
                Type.LineOfSight => new LineOfSightRanger(weapon),
                Type.FloodFill => new FloodFillRanger(),
                _ => throw new ArgumentOutOfRangeException(),
            };
        });
    }

    public override void Activate()
    {
        passabilityLayer = Owner.Game.PassabilityObserver.GetLayer(Passability.Projectile);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public ImmutableArray<Tile> GetTilesInRange()
    {
        ensureTilesInRangeUpToDate();
        return tilesInRange;
    }

    private void ensureTilesInRangeUpToDate()
    {
        var rangeChanged = currentRange != Parameters.Range
            || !currentMaxTurningAngle.Equals(weapon.MaximumTurningAngle)
            || currentTile != Level.GetTile(weapon.Position)
            || tilesInRange.IsDefault;

        if (rangeChanged || Owner.Game.Time >= nextTileInRangeRecalculationTime)
        {
            recalculateTilesInRange();
        }
    }

    private void recalculateTilesInRange()
    {
        currentMaxTurningAngle = weapon.MaximumTurningAngle;
        currentRange = Parameters.Range;
        currentTile = Level.GetTile(weapon.Position);

        tilesInRange = ranger.GetTilesInRange(
            Owner.Game, passabilityLayer, currentTile, Parameters.MinimumRange, Parameters.Range
            );

        nextTileInRangeRecalculationTime = Owner.Game.Time + Parameters.ReCalculateTilesInRangeInterval;
    }
}

