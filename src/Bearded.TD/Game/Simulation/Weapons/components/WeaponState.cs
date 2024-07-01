using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IWeaponState : IPositionable, IDirected
{
    bool IsEnabled { get; }
    Direction2 NeutralDirection { get; }
    Angle? MaximumTurningAngle { get; }
    ITargetingMode TargetingMode { get; }

    void Turn(Angle angle);
    void Disable(IWeaponDisabledReason reason);
    void InjectEvent<T>(T e)
        where T : struct, IComponentEvent;
}

sealed class WeaponState : Component, IWeaponState
{
    private readonly ITurret turret;

    private Angle currentDirectionOffset;
    public Direction2 Direction => turret.NeutralDirection + currentDirectionOffset;

    public Direction2 NeutralDirection => turret.NeutralDirection;
    public Angle? MaximumTurningAngle => turret.MaximumTurningAngle;
    public ITargetingMode TargetingMode => turret.TargetingMode;

    public Position3 Position => turret.Position;

    private readonly List<IWeaponDisabledReason> disabledReasons = new();
    public bool IsEnabled => disabledReasons.Count == 0;

    public WeaponState(ITurret turret)
    {
        this.turret = turret;
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        Owner.Direction = Direction;
        Owner.Position = Position;
        disabledReasons.RemoveAll(r => r.IsResolved);
    }

    public void Turn(Angle angle)
    {
        var newDirection = Direction + angle;
        var newAngleOffset = newDirection - turret.NeutralDirection;

        currentDirectionOffset = MaximumTurningAngle is { } maxAngle
            ? newAngleOffset.Clamped(-maxAngle, maxAngle)
            : newAngleOffset;
    }

    public void Disable(IWeaponDisabledReason reason)
    {
        disabledReasons.Add(reason);
    }

    public void InjectEvent<T>(T e) where T : struct, IComponentEvent
    {
        Events.Send(e);
    }
}
