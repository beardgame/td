using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class FeatureArranger
    {
        private readonly int tilemapRadius;

        public FeatureArranger(int tilemapRadius)
        {
            this.tilemapRadius = tilemapRadius;
        }

        public void ArrangeFeatures(IEnumerable<IFeatureWithArea> features)
        {
            var featuresEnumerated = features as ICollection<IFeatureWithArea> ?? features.ToList();

            var circles = getAllCircles(featuresEnumerated);
            var springs = getAllSprings(featuresEnumerated);

            var system = new SpringSystem(circles, springs, tilemapRadius.U());

            system.Relax();
        }

        private IEnumerable<Spring> getAllSprings(ICollection<IFeatureWithArea> features)
        {
            var nodesWithTheirCircles = features
                .Where(f => f.Feature is NodeFeature)
                .Select(f => (Feature: f, ((CirclesArea) f.Area).Circles))
                .ToList();

            return features.SelectMany(
                f => f.Feature switch
                {
                    ConnectionFeature when f.Area is LineSegmentArea line =>
                        new Spring(line.From, line.To, SpringBehavior.Pull).Yield(),
                    CreviceFeature when f.Area is CirclesArea(var circles) =>
                        nodesWithTheirCircles
                            .SelectMany(n => pushAll(circles, n.Circles, 1.U()))
                            .Concat(Enumerable.Range(0, circles.Length - 1).Select(
                                i => new Spring(circles[i], circles[i + 1], SpringBehavior.Pull, 5, 0.5.U())
                            )),
                    NodeFeature when f.Area is CirclesArea area =>
                        nodesWithTheirCircles
                            .TakeWhile(n => n.Feature != f)
                            .SelectMany(n => pushAll(area.Circles, n.Circles)),
                    _ => throw new InvalidOperationException($"Did not expect {f.Feature.GetType()} with {f.Area.GetType()}")
                });
        }

        private IEnumerable<Spring> pushAll(
            ImmutableArray<RelaxationCircle> circles1,
            ImmutableArray<RelaxationCircle> circles2,
            Unit overlap = default)
        {
            return
                from c1 in circles1
                from c2 in circles2
                select new Spring(c1, c2, SpringBehavior.Push, Overlap: overlap);
        }

        private IEnumerable<RelaxationCircle> getAllCircles(IEnumerable<IFeatureWithArea> features)
        {
            return features.SelectMany(
                f => f.Area is CirclesArea area
                    ? area.Circles
                    : Enumerable.Empty<RelaxationCircle>());
        }
    }
}
