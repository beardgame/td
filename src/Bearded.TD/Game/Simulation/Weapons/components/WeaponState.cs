﻿using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IWeaponState : IPositionable, IDirected
{
    bool IsEnabled { get; }
    TileRangeDrawer.RangeDrawStyle RangeDrawStyle { get; }
    Direction2 NeutralDirection { get; }
    Angle? MaximumTurningAngle { get; }

    void Turn(Angle angle);
    void Enable();
    void Disable();
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

    public Position3 Position => turret.Position;

    public bool IsEnabled { get; private set; }

    public TileRangeDrawer.RangeDrawStyle RangeDrawStyle =>
        turret.BuildingState?.RangeDrawing ?? TileRangeDrawer.RangeDrawStyle.DoNotDraw;

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
    }

    public void Turn(Angle angle)
    {
        var newDirection = Direction + angle;
        var newAngleOffset = newDirection - turret.NeutralDirection;

        currentDirectionOffset = MaximumTurningAngle is { } maxAngle
            ? newAngleOffset.Clamped(-maxAngle, maxAngle)
            : newAngleOffset;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void InjectEvent<T>(T e) where T : struct, IComponentEvent
    {
        Events.Send(e);
    }
}