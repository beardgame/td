using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics;

sealed class ParabolicMovement : Component, IDirected3
{
    private Velocity3 velocity;

    public Difference3 Direction => velocity * 1.S();

    public ParabolicMovement(Velocity3 velocity)
    {
        this.velocity = velocity;
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var forces = Constants.Game.Physics.Gravity3;

        var position = Owner.Position;
        velocity += forces * elapsedTime;

        var step = velocity * elapsedTime;
        var ray = new Ray3(position, step);

        var (result, rayFactor, _, enemy, lastStep, normal) = Owner.Game.Level.CastRayAgainstEnemies(
            ray, Owner.Game.UnitLayer, Owner.Game.PassabilityManager.GetLayer(Passability.Projectile));

        position += step * rayFactor;

        Owner.Position = position;

        var tile = Level.GetTile(position.XY());

        switch (result)
        {
            case RayCastResultType.HitNothing:
                if (position.Z < Owner.Game.GeometryLayer[tile].DrawInfo.Height)
                    hitLevel(position, null);
                break;
            case RayCastResultType.HitLevel:
                hitLevel(position, lastStep);
                break;
            case RayCastResultType.HitEnemy:
                _ = enemy ?? throw new InvalidOperationException();
                hitEnemy(position, enemy, normal.Value);
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    private void hitEnemy(Position3 position, GameObject enemy, Difference3 normal)
    {
        var info = new HitInfo(position, normal, velocityVector());
        Events.Send(new HitEnemy(enemy, info));
        Owner.Delete();
    }

    private void hitLevel(Position3 point, Direction? withStep)
    {
        var normal = new Difference3(withStep?.Vector().WithZ() ?? Vector3.UnitZ);
        var info = new HitInfo(point, normal, velocityVector());
        Events.Send(new HitLevel(info));
        Owner.Delete();
    }

    private Difference3 velocityVector() => new(velocity.NumericValue.NormalizedSafe());
}
