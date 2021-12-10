using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface ITurret : IPositionable
{
    Weapon Weapon { get; }
    IGameObject Owner { get; }
    IBuildingState? BuildingState { get; }
    Direction2 NeutralDirection { get; }
    Angle? MaximumTurningAngle { get; }
    void OverrideTargeting(IManualTarget3 target);
    void StopTargetOverride();
}

[Component("turret")]
sealed class Turret<T> : Component<T, ITurretParameters>, ITurret, INestedComponentOwner
    where T : IComponentOwner, IGameObject, IPositionable
{
    private Weapon weapon = null!;
    private ITransformable transform = null!;
    private TargetOverride? targetOverride;

    public IBuildingState? BuildingState { get; private set; }
    public Position3 Position =>
        (Owner.Position.XY() + transform.LocalCoordinateTransform.Transform(Parameters.Offset))
        .WithZ(Owner.Position.Z + Parameters.Height);

    public Direction2 NeutralDirection => Parameters.NeutralDirection + transform.LocalOrientationTransform;
    public Angle? MaximumTurningAngle => Parameters.MaximumTurningAngle;

    public IComponentOwner NestedComponentOwner => weapon;

    public Turret(ITurretParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        weapon = new Weapon(Parameters.Weapon, this);
        transform = Owner.GetComponents<ITransformable>().FirstOrDefault() ?? Transformable.Identity;
        ComponentDependencies.Depend<IBuildingStateProvider>(
            Owner, Events, provider => BuildingState = provider.State);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        weapon.Update(elapsedTime);
    }

    public void OverrideTargeting(IManualTarget3 target)
    {
        StopTargetOverride();

        targetOverride = new TargetOverride(target);
        weapon.AddComponent(targetOverride);
    }

    public void StopTargetOverride()
    {
        if (targetOverride == null)
            return;

        weapon.RemoveComponent(targetOverride);
        targetOverride = null;
    }

    Weapon ITurret.Weapon => weapon;
    IGameObject ITurret.Owner => Owner;

    public override bool CanApplyUpgradeEffect(IUpgradeEffect effect)
    {
        return base.CanApplyUpgradeEffect(effect) || weapon.CanApplyUpgradeEffect(effect);
    }

    public override void ApplyUpgradeEffect(IUpgradeEffect effect)
    {
        base.ApplyUpgradeEffect(effect);
        weapon.ApplyUpgradeEffect(effect);
    }

    public override bool RemoveUpgradeEffect(IUpgradeEffect effect)
    {
        var removed = false;
        removed |= base.RemoveUpgradeEffect(effect);
        removed |= weapon.RemoveUpgradeEffect(effect);
        return removed;
    }

    private sealed class TargetOverride : Component<Weapon>,
        IPositionable, IWeaponAimer, ITargeter<IPositionable>, IWeaponTrigger
    {
        private readonly IManualTarget3 target;

        IPositionable? ITargeter<IPositionable>.Target => this;

        public Direction2 AimDirection { get; private set; }
        public Position3 Position { get; private set; }
        public bool TriggerPulled { get; private set; }

        public TargetOverride(IManualTarget3 target)
        {
            this.target = target;
        }

        protected override void OnAdded()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            Position = target.Target;
            AimDirection = Direction2.Between(Owner.Position.NumericValue.Xy, Position.NumericValue.Xy);
            TriggerPulled = target.TriggerPulled;
        }
    }
}