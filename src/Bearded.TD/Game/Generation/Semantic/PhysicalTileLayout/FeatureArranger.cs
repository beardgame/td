using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout.PhysicalFeature;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

sealed class FeatureArranger
{
    private readonly int tilemapRadius;

    public FeatureArranger(int tilemapRadius)
    {
        this.tilemapRadius = tilemapRadius;
    }

    public List<PhysicalFeature> ArrangeFeatures(IEnumerable<PhysicalFeature> features)
    {
        var featuresEnumerated = features as ICollection<PhysicalFeature> ?? features.ToList();

        var circles = getAllCircles(featuresEnumerated);
        var springs = getAllSprings(featuresEnumerated);

        var system = SpringSystem.From(circles, springs, tilemapRadius.U());

        system.Relax();

        var finalCircles = system.GetCurrentCircles();

        return featuresWithCircles(featuresEnumerated, finalCircles).ToList();
    }

    private IEnumerable<Spring> getAllSprings(ICollection<PhysicalFeature> features)
    {
        var allNodes = features.OfType<Node>();

        return features.SelectMany(
            f => f switch
            {
                Connection(var from, var to, _) =>
                    new Spring(from, to, SpringBehavior.Pull).Yield(),
                Crevice crevice =>
                    allNodes
                        .SelectMany(n => pushAll(crevice.FeatureCircles, n.FeatureCircles, 1.U()))
                        .Concat(
                            crevice.FeatureCircles
                                .ConsecutivePairs((c0, c1) => new Spring(c0, c1, SpringBehavior.Pull, 5, 0.5.U()))),
                Node node =>
                    allNodes
                        .TakeWhile(n => n != f)
                        .SelectMany(n => pushAll(node.FeatureCircles, n.FeatureCircles)),
                _ => throw new InvalidOperationException()
            });
    }

    private IEnumerable<Spring> pushAll(
        IEnumerable<FeatureCircle> circles1,
        IEnumerable<FeatureCircle> circles2,
        Unit overlap = default)
    {
        return
            from c1 in circles1
            from c2 in circles2
            select new Spring(c1, c2, SpringBehavior.Push, Overlap: overlap);
    }

    private IEnumerable<FeatureCircle> getAllCircles(IEnumerable<PhysicalFeature> features)
    {
        return features.OfType<WithCircles>().SelectMany(f => f.FeatureCircles);
    }

    private IEnumerable<PhysicalFeature> featuresWithCircles(
        ICollection<PhysicalFeature> features, ImmutableArray<Circle> circles)
    {
        var circlesUsed = 0;

        var newCircleFeatures = features.OfType<WithCircles>()
            .Select(f => KeyValuePair.Create(f, f with {Circles = nextCircles(f.Circles.Length)}))
            .ToDictionary();

        return features.Select(f => f switch
        {
            WithCircles circleFeature => newCircleFeatures[circleFeature],
            Connection connection => newConnection(connection),
            _ => throw new InvalidOperationException()
        });

        PhysicalFeature newConnection(Connection old)
        {
            var (from, to, width) = old;
            return new Connection(
                new FeatureCircle(newCircleFeatures[from.Feature], from.Index),
                new FeatureCircle(newCircleFeatures[to.Feature], to.Index),
                width
            );
        }

        ImmutableArray<Circle> nextCircles(int count)
        {
            var cs = ImmutableArray.Create(circles, circlesUsed, count);
            circlesUsed += count;
            return cs;
        }
    }
}
