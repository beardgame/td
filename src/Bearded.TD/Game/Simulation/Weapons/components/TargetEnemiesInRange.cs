using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("targetEnemiesInRange")]
sealed partial class TargetEnemiesInRange
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
        Angle? ConeOfFire { get; }
        [Modifiable(0.2)] TimeSpan NoTargetIdleInterval { get; }
        [Modifiable(1)] TimeSpan ReCalculateTilesInRangeInterval { get; }
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

    private bool skipDrawThisFrame;

    public bool TriggerPulled { get; private set; }
    public Direction2 AimDirection { get; private set; }

    public Unit Range => currentRange;

    public TargetEnemiesInRange(IParameters parameters) : base(parameters) {}

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
        if (!weapon.IsEnabled)
        {
            turnOff();
            return;
        }

        if (endOfIdleTime > Owner.Game.Time)
        {
            return;
        }

        tryShootingAtTarget();
    }

    private void turnOff()
    {
        TriggerPulled = false;
    }

    private void tryShootingAtTarget()
    {
        ensureTilesInRangeUpToDate();
        ensureTargetValid();

        if (target == null)
        {
            goIdle();
            return;
        }

        updateTargetPosition(target);
        shootAtTargetPosition();
    }

    private void goIdle()
    {
        if (weapon.MaximumTurningAngle != null)
        {
            AimDirection = weapon.NeutralDirection;
        }

        TriggerPulled = false;

        endOfIdleTime = Owner.Game.Time + Parameters.NoTargetIdleInterval;
    }

    private void updateTargetPosition(GameObject target)
    {
        if (emitter == null || target.GetComponents<IMoving>().FirstOrDefault() is not { } moving)
        {
            targetPosition.Position = target.Position;
            return;
        }

        var distanceToTarget = (target.Position - emitter.EmitPosition).Length;
        var projectileFlightTime = new TimeSpan(distanceToTarget.NumericValue / emitter.MuzzleSpeed.NumericValue);
        var compensationOffset = moving.Velocity * projectileFlightTime;

        targetPosition.Position = target.Position + compensationOffset;
    }

    private void shootAtTargetPosition()
    {
        var emitPosition = emitter?.EmitPosition ?? Owner.Position;

        AimDirection = (targetPosition.Position - emitPosition).XY().Direction;

        if (isAimDirectionInConeOfFire())
        {
            TriggerPulled = true;
        }
    }

    private bool isAimDirectionInConeOfFire()
    {
        if (Parameters.ConeOfFire == null)
        {
            return true;
        }

        var angleError = Angle.Between(AimDirection, weapon.Direction).Abs();
        return angleError < Parameters.ConeOfFire;
    }
}
