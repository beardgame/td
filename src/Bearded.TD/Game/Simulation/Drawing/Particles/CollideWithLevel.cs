using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesCollideWithLevel")]
sealed class CollideWithLevel : ParticleUpdater<CollideWithLevel.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        float CollisionNormalFactor { get; }
        [Modifiable(1)]
        float CollisionTangentFactor { get; }
    }

    public CollideWithLevel(IParameters parameters) : base(parameters)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var geometry = Owner.Game.GeometryLayer;
        var level = Owner.Game.Level;

        foreach (ref var particle in Particles.MutableParticles)
        {
            collideWithLevel(
                geometry,
                level,
                ref particle.Position,
                ref particle.Velocity,
                elapsedTime);
        }
    }

    private void collideWithLevel(
        GeometryLayer geometry,
        Level level,
        ref Position3 p,
        ref Velocity3 v,
        TimeSpan elapsedTime)
    {
        var step = v * elapsedTime;

        var rayCaster = new LevelRayCaster();
        rayCaster.StartEnumeratingTiles(new Ray(p.XY(), step.XY()));

        var previousRayFactor = 0f;
        rayCaster.MoveNext(out var tile);

        while (true)
        {
            if (!level.IsValid(tile))
                break;

            var info = geometry[tile];
            var tileType = info.Geometry.Type;

            var tileHeight = tileType switch {
                TileType.Floor => info.DrawInfo.Height,
                TileType.Wall => float.PositiveInfinity.U(),
                TileType.Crevice => float.NegativeInfinity.U(),
                _ => throw new ArgumentOutOfRangeException(),
            };

            var lastTile = !rayCaster.MoveNext(out tile);
            var rayFactor = lastTile ? 1 : rayCaster.CurrentRayFactor;

            var floorCollisionFactor = (tileHeight - p.Z) / step.Z;
            if (floorCollisionFactor > previousRayFactor && floorCollisionFactor <= rayFactor)
            {
                reflect(ref p, ref v, elapsedTime, floorCollisionFactor, Vector3.UnitZ);
                return;
            }

            if (lastTile)
                break;

            var heightAtTileBorder = p.Z + step.Z * rayFactor;

            if (heightAtTileBorder < tileHeight)
            {
                reflect(ref p, ref v, elapsedTime, rayFactor, (-rayCaster.LastStep!.Value.Vector()).WithZ());
                return;
            }

            previousRayFactor = rayFactor;
        }
    }

    private void reflect(ref Position3 p, ref Velocity3 v, TimeSpan elapsedTime, float factor, Vector3 normal)
    {
        normal = normal.NormalizedSafe();

        var dotWithVelocityOutMagnitude = Vector3.Dot(normal, -v.NumericValue);

        var normalVelocityOut = new Velocity3(normal * dotWithVelocityOutMagnitude);
        var tangentVelocity = v + normalVelocityOut;

        var velocityOut = normalVelocityOut * Parameters.CollisionNormalFactor + tangentVelocity * Parameters.CollisionNormalFactor;

        var stepToCollisionPoint = v * elapsedTime * factor;
        var stepBackwardsFromCollisionPoint = velocityOut * elapsedTime;
        var bias = normal * 0.0001.U();

        p = p + bias + stepToCollisionPoint - stepBackwardsFromCollisionPoint;
        v = velocityOut;
    }
}

