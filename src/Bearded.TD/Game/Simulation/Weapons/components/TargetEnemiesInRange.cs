using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("targetEnemiesInRange")]
sealed class TargetEnemiesInRange
    : Component<TargetEnemiesInRange.IParameters>,
        IWeaponRangeDrawer,
        ITargeter<IPositionable>,
        IWeaponAimer,
        IWeaponTrigger,
        IWeaponRange,
        IListener<DrawComponents>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.Range)] Unit Range { get; }
        Unit MinimumRange { get; }
        [Modifiable(0.2)] TimeSpan NoTargetIdleInterval { get; }
        [Modifiable(1)] TimeSpan ReCalculateTilesInRangeInterval { get; }

        Angle? ConeOfFire { get; }
    }

    private PassabilityLayer passabilityLayer = null!;
    private TileRangeDrawer tileRangeDrawer = null!;
    private IWeaponState weapon = null!;
    private IProjectileEmitter? emitter;

    // mutable state
    private Instant endOfIdleTime;
    private Instant nextTileInRangeRecalculationTime;

    private Angle? currentMaxTurningAngle;
    private Unit currentRange;
    private ImmutableArray<Tile> tilesInRange = ImmutableArray<Tile>.Empty;

    private readonly Positionable targetPosition = new();
    private GameObject? target;
    public IPositionable? Target => target == null ? null : targetPosition;

    private bool dontDrawThisFrame;

    public bool TriggerPulled { get; private set; }
    public Direction2 AimDirection { get; private set; }

    public Unit Range => currentRange;

    public TargetEnemiesInRange(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => weapon = c);
        ComponentDependencies.Depend<IProjectileEmitter>(Owner, Events, e => emitter = e);
    }

    public override void Activate()
    {
        base.Activate();

        passabilityLayer = Owner.Game.PassabilityManager.GetLayer(Passability.Projectile);
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
        tryShootingAtTarget();
    }

    private void tryShootingAtTarget()
    {
        if (!weapon.IsEnabled)
        {
            turnOff();
            return;
        }

        if (endOfIdleTime > Owner.Game.Time)
            return;

        ensureTilesInRangeList();
        ensureTargetingState();

        if (target == null)
        {
            goIdle();
            return;
        }

        updateTargetPosition(target);
        shootAt(targetPosition.Position);
    }

    private void updateTargetPosition(GameObject target)
    {
        if (emitter == null ||
            target.GetComponents<IMoving>().FirstOrDefault() is not { } moving)
        {
            targetPosition.Position = target.Position;
            return;
        }

        var distanceToTarget = (target.Position - Owner.Position).Length;
        var projectileFlightTime = new TimeSpan(distanceToTarget.NumericValue / emitter.MuzzleSpeed.NumericValue);
        var compensationOffset = moving.Velocity * projectileFlightTime;

        targetPosition.Position = target.Position + compensationOffset;
    }

    private void shootAt(Position3 targetPosition)
    {
        AimDirection = (targetPosition - Owner.Position).XY().Direction;

        if (Parameters.ConeOfFire == null)
        {
            TriggerPulled = true;
            return;
        }
        var angleError = Angle.Between(AimDirection, weapon.Direction).Abs();
        var inConeOfFire = angleError < Parameters.ConeOfFire;
        if (inConeOfFire)
        {
            TriggerPulled = true;
        }
    }

    private void goIdle()
    {
        if(weapon.MaximumTurningAngle != null)
            AimDirection = weapon.NeutralDirection;
        TriggerPulled = false;

        endOfIdleTime = Owner.Game.Time + Parameters.NoTargetIdleInterval;
    }

    private void turnOff()
    {
        TriggerPulled = false;
    }

    private void ensureTilesInRangeList()
    {
        if (currentRange == Parameters.Range
            && currentMaxTurningAngle.Equals(weapon.MaximumTurningAngle)
            && nextTileInRangeRecalculationTime > Owner.Game.Time)
            return;

        recalculateTilesInRange();
    }

    private void recalculateTilesInRange()
    {
        currentMaxTurningAngle = weapon.MaximumTurningAngle;
        currentRange = Parameters.Range;
        var rangeSquared = currentRange.Squared;
        var minRangeSquared = Parameters.MinimumRange.Squared;

        var level = Owner.Game.Level;
        var navigator = Owner.Game.Navigator;

        var visibilityChecker = currentMaxTurningAngle is { } maxAngle
            ? new LevelVisibilityChecker().InDirection(weapon.NeutralDirection, maxAngle)
            : new LevelVisibilityChecker();

        tilesInRange = visibilityChecker.EnumerateVisibleTiles(
                level,
                Owner.Position.XY(),
                currentRange,
                t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
            .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                (Level.GetPosition(t.tile) - Owner.Position.XY()).LengthSquared is var dSquared &&
                dSquared >= minRangeSquared && dSquared <= rangeSquared)
            .Select(t => t.tile)
            .OrderBy(navigator.GetDistanceToClosestSink)
            .ToImmutableArray();

        nextTileInRangeRecalculationTime = Owner.Game.Time + Parameters.ReCalculateTilesInRangeInterval;
    }

    private void ensureTargetingState()
    {
        if (target?.Deleted == true)
            target = null;

        // TODO: accumulating tiles each frame is expensive, can we somehow cache this?
        if (target != null && !tilesInRange.OverlapsWithTiles(OccupiedTileAccumulator.AccumulateOccupiedTiles(target)))
            target = null;

        if (target != null)
            return;

        tryFindTarget();
    }

    private void tryFindTarget()
    {
        target = tilesInRange
            .SelectMany(Owner.Game.UnitLayer.GetUnitsOnTile)
            .FirstOrDefault();
    }

    public void HandleEvent(DrawComponents e)
    {
        tileRangeDrawer.Draw();
    }

    private IEnumerable<Tile> getTilesToDraw()
    {
        if (dontDrawThisFrame)
        {
            dontDrawThisFrame = false;
            return ImmutableHashSet<Tile>.Empty;
        }

        var allTiles = ImmutableHashSet.CreateRange(
            (Owner.Parent?.GetComponents<ITurret>() ?? Enumerable.Empty<ITurret>())
            .Select(t => t.Weapon)
            .SelectMany(w => w.GetComponents<IWeaponRangeDrawer>())
            .SelectMany(ranger => ranger.TakeOverDrawingThisFrame()));
        dontDrawThisFrame = false;
        return allTiles;
    }

    IEnumerable<Tile> IWeaponRangeDrawer.TakeOverDrawingThisFrame()
    {
        dontDrawThisFrame = true;
        recalculateTilesInRange();
        return tilesInRange;
    }
}
