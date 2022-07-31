using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface ITurret : IPositionable
{
    GameObject Weapon { get; }
    IGameObject Owner { get; }
    IBuildingState? BuildingState { get; }
    Direction2 NeutralDirection { get; }
    Angle? MaximumTurningAngle { get; }
    void OverrideTargeting(IManualTarget3 target);
    void StopTargetOverride();
}

[Component("turret")]
sealed class Turret : Component<Turret.IParameters>, ITurret, IListener<DrawComponents>, IListener<ObjectDeleting>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IComponentOwnerBlueprint Weapon { get; }
        Difference2 Offset { get; }

        [Modifiable(0.25)]
        Unit Height { get; }

        Direction2 NeutralDirection { get; }
        Angle? MaximumTurningAngle { get; }
    }


    public GameObject Weapon { get; private set; } = null!;
    private IWeaponState weaponState = null!;
    private ITransformable transform = null!;
    private TargetOverride? targetOverride;
    private bool? previouslyFunctional;

    public IBuildingState? BuildingState { get; private set; }
    public Position3 Position =>
        (Owner.Position.XY() + transform.LocalCoordinateTransform.Transform(Parameters.Offset))
        .WithZ(Owner.Position.Z + Parameters.Height);

    public Direction2 NeutralDirection => Parameters.NeutralDirection + transform.LocalOrientationTransform;
    public Angle? MaximumTurningAngle => Parameters.MaximumTurningAngle;

    public Turret(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Weapon = WeaponFactory.Create(this, Parameters.Weapon);
        weaponState = Weapon.GetComponents<IWeaponState>().Single();
        transform = Owner.GetComponents<ITransformable>().FirstOrDefault() ?? Transformable.Identity;
        ComponentDependencies.Depend<IBuildingStateProvider>(
            Owner, Events, provider => BuildingState = provider.State);

        Events.Subscribe<DrawComponents>(this);
        Events.Subscribe<ObjectDeleting>(this);
    }

    public override void Activate()
    {
        base.Activate();
        Owner.Game.Add(Weapon);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<DrawComponents>(this);
        Events.Unsubscribe<ObjectDeleting>(this);
    }

    public void HandleEvent(DrawComponents e)
    {
        weaponState.InjectEvent(e);
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        Weapon.Delete();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        updateFunctional();
    }

    private void updateFunctional()
    {
        var currentlyFunctional = BuildingState?.IsFunctional ?? true;

        if (currentlyFunctional == previouslyFunctional)
            return;

        if (currentlyFunctional)
            weaponState.Enable();
        else
            weaponState.Disable();

        previouslyFunctional = currentlyFunctional;
    }

    public void OverrideTargeting(IManualTarget3 target)
    {
        StopTargetOverride();

        targetOverride = new TargetOverride(target);
        Weapon.AddComponent(targetOverride);
    }

    public void StopTargetOverride()
    {
        if (targetOverride == null)
            return;

        Weapon.RemoveComponent(targetOverride);
        targetOverride = null;
    }

    IGameObject ITurret.Owner => Owner;

    public override bool CanApplyUpgradeEffect(IUpgradeEffect effect)
    {
        return base.CanApplyUpgradeEffect(effect) || Weapon.CanApplyUpgradeEffect(effect);
    }

    public override void ApplyUpgradeEffect(IUpgradeEffect effect)
    {
        base.ApplyUpgradeEffect(effect);
        Weapon.ApplyUpgradeEffect(effect);
    }

    public override bool RemoveUpgradeEffect(IUpgradeEffect effect)
    {
        var removed = false;
        removed |= base.RemoveUpgradeEffect(effect);
        removed |= Weapon.RemoveUpgradeEffect(effect);
        return removed;
    }

    private sealed class TargetOverride : Component,
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
