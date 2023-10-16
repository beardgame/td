using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnConnectingSourceAndTarget")]
sealed class SpawnConnectingSourceAndTarget : ParticleUpdater<SpawnConnectingSourceAndTarget.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        [Modifiable(1)]
        Unit SegmentLength { get; }
    }

    public SpawnConnectingSourceAndTarget(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

        if (!Owner.TryGetSingleComponent<IProperty<Source>>(out var source) ||
            !Owner.TryGetSingleComponent<IProperty<Target>>(out var target))
        {
            Owner.Game.Meta.Logger.Debug?
                .Log($"{nameof(SpawnConnectingSourceAndTarget)} requires a source and target property.");
            return;
        }

        var sourcePosition = source.Value.Object.Position;
        var targetPosition = target.Value.Object.Position;
        var sourceTile = Level.GetTile(sourcePosition);
        var targetTile = Level.GetTile(targetPosition);

        var passability = Owner.Game.PassabilityObserver.GetLayer(Passability.Projectile);

        var pathfinder = Pathfinder.WithTileCosts(t => passability[t].IsPassable ? StaticRandom.Double(1, 3) : 20, 1);

        var result = pathfinder.FindPath(sourceTile, targetTile);

        if (result is not { Path: var path })
        {
            Owner.Game.Meta.Logger.Debug?
                .Log($"{nameof(SpawnConnectingSourceAndTarget)} could not find a path.");
            return;
        }

        var secondTile = path.Length == 0 ? sourceTile : sourceTile.Neighbor(path[0]);
        Unit firstStepLength;
        Unit pathLength;

        if (path.Length <= 1)
        {
            firstStepLength = (sourcePosition.XY() - targetPosition.XY()).Length;
            pathLength = firstStepLength;
        }
        else
        {
            firstStepLength = (sourcePosition.XY() - Level.GetPosition(secondTile)).Length;
            var secondToLastTile = targetTile.Neighbor(path[^1].Opposite());
            var lastStepLength = (targetPosition.XY() - Level.GetPosition(secondToLastTile)).Length;

            pathLength = firstStepLength + lastStepLength + (path.Length - 2).U();
        }

        var particleCount = Math.Max(2, MoreMath.CeilToInt(pathLength / Parameters.SegmentLength) + 1);
        var particles = Particles.CreateParticles(
            Parameters, Velocity3.Zero, Direction2.Zero, Owner.Game.Time, Position3.Zero,
            out var transaction, particleCount);

        particles[0].Position = sourcePosition;
        particles[^1].Position = targetPosition;

        var realSegmentLength = pathLength / (particleCount - 1);
        var distanceTraveled = 0.U();
        var nextTile = secondTile;
        var previousPoint = sourcePosition.XY();
        var nextPoint = Level.GetPosition(nextTile);
        var previousPointDistanceFromSource = 0.U();
        var nextPointDistanceFromSource = firstStepLength;
        var step = 1;

        foreach (ref var particle in particles.Slice(1, particles.Length - 2))
        {
            distanceTraveled += realSegmentLength;
            while (step <= path.Length && distanceTraveled > nextPointDistanceFromSource)
            {
                previousPoint = nextPoint;
                previousPointDistanceFromSource = nextPointDistanceFromSource;
                if (step == path.Length)
                {
                    nextPoint = targetPosition.XY();
                    nextPointDistanceFromSource = pathLength;
                }
                else
                {
                    nextTile = nextTile.Neighbor(path[step]);
                    nextPoint = Level.GetPosition(nextTile);
                    nextPointDistanceFromSource += 1.U();
                }
                step++;
            }
            var t = (distanceTraveled - previousPointDistanceFromSource) / (nextPointDistanceFromSource - previousPointDistanceFromSource);
            var zT = distanceTraveled / pathLength;
            var z = sourcePosition.Z + (targetPosition.Z - sourcePosition.Z) * zT;
            particle.Position += (previousPoint + (nextPoint - previousPoint) * t).WithZ(z) - Position3.Zero;
        }

        transaction.Commit();
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

