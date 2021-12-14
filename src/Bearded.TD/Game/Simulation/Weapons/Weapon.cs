using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[ComponentOwner]
sealed class Weapon : IGameObject, IPositionable, IDirected, IComponentOwner<Weapon>
{
    private readonly ITurret turret;

    private readonly ComponentCollection<Weapon> components;
    private readonly ComponentEvents events = new();

    private Angle currentDirectionOffset;
    public Direction2 CurrentDirection => turret.NeutralDirection + currentDirectionOffset;

    public Direction2 NeutralDirection => turret.NeutralDirection;
    public Angle? MaximumTurningAngle => turret.MaximumTurningAngle;

    public IComponentOwner? Parent => (IComponentOwner)turret.Owner;
    public IGameObject Owner => turret.Owner;
    public Position3 Position => turret.Position;

    public GameState Game => Owner.Game;

    // TODO: this should not be revealed here
    public TileRangeDrawer.RangeDrawStyle RangeDrawStyle =>
        turret.BuildingState?.RangeDrawing ?? TileRangeDrawer.RangeDrawStyle.DoNotDraw;

    public Weapon(IComponentOwnerBlueprint blueprint, ITurret turret)
    {
        this.turret = turret;

        components = new ComponentCollection<Weapon>(this, events);
        components.Add(blueprint.GetComponents<Weapon>());
    }

    public bool CanApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.CanApplyTo(this);

    public void ApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.ApplyTo(this);

    public bool RemoveUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.RemoveFrom(this);

    public void Turn(Angle angle)
    {
        var newDirection = CurrentDirection + angle;
        var newAngleOffset = newDirection - turret.NeutralDirection;

        currentDirectionOffset = MaximumTurningAngle is { } maxAngle
            ? newAngleOffset.Clamped(-maxAngle, maxAngle)
            : newAngleOffset;
    }

    public void Update(TimeSpan elapsedTime)
    {
        if (turret.BuildingState is not { IsFunctional: true })
        {
            return;
        }

        components.Update(elapsedTime);
    }

    public void AddComponent(IComponent<Weapon> component) => components.Add(component);

    public void RemoveComponent(IComponent<Weapon> component) => components.Remove(component);

    IEnumerable<TComponent> IComponentOwner<Weapon>.GetComponents<TComponent>() => components.Get<TComponent>();

    IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();

    Direction2 IDirected.Direction => CurrentDirection;
}
