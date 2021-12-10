using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

sealed class SpringSystem
{
    readonly struct Spring
    {
        public int Circle1Index { get; }
        public int Circle2Index { get; }
        public Unit TargetDistance { get; }
        public SpringBehavior Behavior { get; }
        public float ForceMultiplier { get; }

        public Spring(
            int circle1Index, int circle2Index, Unit targetDistance, SpringBehavior behavior, float forceMultiplier)
        {
            Circle1Index = circle1Index;
            Circle2Index = circle2Index;
            TargetDistance = targetDistance;
            Behavior = behavior;
            ForceMultiplier = forceMultiplier;
        }
    }

    private readonly Unit radius;
    private readonly Circle[] circles;
    private readonly ImmutableArray<Spring> springs;
    private readonly Difference2[] forceAccumulator;

    public static SpringSystem From(
        IEnumerable<FeatureCircle> featureCircles, IEnumerable<PhysicalTileLayout.Spring> featureSprings, Unit radius)
    {
        var featureCirclesEnumerated = featureCircles as ICollection<FeatureCircle> ?? featureCircles.ToList();

        var circles = featureCirclesEnumerated.Select(fc => fc.Circle).ToArray();
        var circleIds = featureCirclesEnumerated.Select(KeyValuePair.Create).ToDictionary();

        var springs = featureSprings.Select(s => new Spring(
            circleIds[s.Circle1], circleIds[s.Circle2], s.TargetDistance, s.Behavior, s.ForceMultiplier
        )).ToImmutableArray();

        return new SpringSystem(radius, circles, springs);
    }

    private SpringSystem(Unit radius, Circle[] circles, ImmutableArray<Spring> springs)
    {
        this.radius = radius;
        this.circles = circles;
        this.springs = springs;
        forceAccumulator = new Difference2[circles.Length];
    }

    public void Relax()
    {
        // TODO: this algorithm needs various improvements:
        // - limit how far a circle can be pushed in a single frame to avoid too fast movement
        // - decrease max movement over time to arrive at stable solution
        // TODO: we could also try increasing performance:
        // - pre-calculate current distances between springs and split push and pull springs
        //   - then, because of known max movement (accumulated), we only have to iterate springs that could possibly overlap
        //   - benchmark if this is actually worth it
        //   - may have to fiddle with how often spring distances are re-calculated for optimal performance
        //     - perhaps make this depend on how many springs were iterated that weren't in range after all
        //     - or just use some sort of min-heap and keep updating it as we iterate so springs get re-sorted all the time

        foreach (var _ in Enumerable.Range(0, 100))
        {
            foreach (var spring in springs)
            {
                var (n1, n2) = (circles[spring.Circle1Index], circles[spring.Circle2Index]);

                var diff = n1.Center - n2.Center;
                var dSquared = diff.LengthSquared;

                var targetD = spring.TargetDistance;
                var targetDSquared = targetD.Squared;

                var springIsActive = spring.Behavior switch
                {
                    SpringBehavior.Push => targetDSquared >= dSquared,
                    SpringBehavior.Pull => targetDSquared <= dSquared,
                    _ => throw new NotSupportedException()
                };

                if (!springIsActive)
                    continue;

                var forceMagnitude = (targetDSquared.NumericValue - dSquared.NumericValue).U() * 0.01f *
                    spring.ForceMultiplier;
                var forceOnN1 = diff / dSquared.Sqrt() * forceMagnitude;

                forceAccumulator[spring.Circle1Index] += forceOnN1;
                forceAccumulator[spring.Circle2Index] -= forceOnN1;
            }

            foreach (var i in Enumerable.Range(0, circles.Length))
            {
                var vertex = circles[i];

                foreach (var direction in Tiles.Extensions.Directions)
                {
                    var lineNormal = direction.CornerAfter();
                    var lineDistance = HexagonDistanceY * radius - vertex.Radius;

                    var projection = Vector2.Dot(lineNormal, vertex.Center.NumericValue).U();

                    if (projection < lineDistance)
                        continue;

                    var forceMagnitude =
                        (lineDistance.NumericValue.Squared() - projection.NumericValue.Squared()).U() * 0.01f;

                    forceAccumulator[i] += lineNormal * forceMagnitude;
                }
            }

            foreach (var i in Enumerable.Range(0, circles.Length))
            {
                var force = forceAccumulator[i];
                var circle = circles[i];

                circles[i] = new Circle(circle.Center + force, circle.Radius);

                forceAccumulator[i] = default;
            }

        }
    }

    public ImmutableArray<Circle> GetCurrentCircles()
    {
        return circles.ToImmutableArray();
    }
}