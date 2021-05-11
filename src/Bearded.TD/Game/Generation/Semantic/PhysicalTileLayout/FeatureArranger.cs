using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout.PhysicalFeature;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class FeatureArranger
    {
        private readonly int tilemapRadius;

        public FeatureArranger(int tilemapRadius)
        {
            this.tilemapRadius = tilemapRadius;
        }

        public void ArrangeFeatures(IEnumerable<PhysicalFeature> features)
        {
            var featuresEnumerated = features as ICollection<PhysicalFeature> ?? features.ToList();

            var circles = getAllCircles(featuresEnumerated);
            var springs = getAllSprings(featuresEnumerated);

            var system = new SpringSystem(circles, springs, tilemapRadius.U());

            system.Relax();
        }

        private IEnumerable<Spring> getAllSprings(ICollection<PhysicalFeature> features)
        {
            var allNodes = features.OfType<Node>();

            return features.SelectMany(
                f => f switch
                {
                    Connection(var from, var to) =>
                        new Spring(from.Circle, to.Circle, SpringBehavior.Pull).Yield(),
                    Crevice(var circles) =>
                        allNodes
                            .SelectMany(n => pushAll(circles, n.Circles, 1.U()))
                            .Concat(Enumerable.Range(0, circles.Length - 1).Select(
                                i => new Spring(circles[i], circles[i + 1], SpringBehavior.Pull, 5, 0.5.U())
                            )),
                    Node(_, var circles) =>
                        allNodes
                            .TakeWhile(n => n != f)
                            .SelectMany(n => pushAll(circles, n.Circles)),
                    _ => throw new NotImplementedException()
                });
        }

        private IEnumerable<Spring> pushAll(
            ImmutableArray<Circle> circles1,
            ImmutableArray<Circle> circles2,
            Unit overlap = default)
        {
            return
                from c1 in circles1
                from c2 in circles2
                select new Spring(c1, c2, SpringBehavior.Push, Overlap: overlap);
        }

        private IEnumerable<Circle> getAllCircles(IEnumerable<PhysicalFeature> features)
        {
            return features.OfType<IFeatureWithCircles>().SelectMany(f => f.Circles);
        }
    }
}
