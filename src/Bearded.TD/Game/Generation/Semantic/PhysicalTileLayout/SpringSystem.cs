using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class SpringSystem
    {
        private readonly Unit radius;
        private readonly ImmutableArray<RelaxationCircle> vertices;
        private readonly ImmutableArray<Spring> springs;

        public SpringSystem(IEnumerable<RelaxationCircle> circles, IEnumerable<Spring> springs, Unit radius)
        {
            this.radius = radius;
            vertices = circles.ToImmutableArray();
            this.springs = springs.ToImmutableArray();
        }

        public void Relax()
        {
            foreach (var _ in Enumerable.Range(0, 100))
            {
                foreach (var spring in springs)
                {
                    var (n1, n2) = (spring.Circle1, spring.Circle2);

                    var diff = n1.Position - n2.Position;
                    var dSquared = diff.LengthSquared;

                    var targetD = spring.TargetDistance;
                    var targetDSquared = targetD.Squared;

                    switch (spring.Behavior)
                    {
                        case SpringBehavior.Push:
                            if (targetDSquared < dSquared)
                                continue;
                            break;
                        case SpringBehavior.Pull:
                            if (targetDSquared > dSquared)
                                continue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var forceMagnitude = (targetDSquared.NumericValue - dSquared.NumericValue).U() * 0.01f *
                        spring.ForceMultiplier;
                    var forceOnN1 = diff / dSquared.Sqrt() * forceMagnitude;

                    n1.Position += forceOnN1;
                    n2.Position -= forceOnN1;
                }

                foreach (var vertex in vertices)
                {
                    foreach (var direction in Tiles.Extensions.Directions)
                    {
                        var lineNormal = direction.CornerAfter();
                        var lineDistance = HexagonDistanceY * radius - vertex.Radius;

                        var projection = Vector2.Dot(lineNormal, vertex.Position.NumericValue).U();

                        if (projection < lineDistance)
                            continue;

                        var forceMagnitude =
                            (lineDistance.NumericValue.Squared() - projection.NumericValue.Squared()).U() * 0.01f;

                        vertex.Position += lineNormal * forceMagnitude;
                    }
                }
            }
        }
    }
}
