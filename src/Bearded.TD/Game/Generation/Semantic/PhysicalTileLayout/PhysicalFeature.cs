using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

abstract record PhysicalFeature
{
    private PhysicalFeature()
    {
    }

    public TiledFeature WithTiles(IEnumerable<Tile> tiles) => new(this, Area.From(tiles));

    public abstract record WithCircles(ImmutableArray<Circle> Circles)
        : PhysicalFeature
    {
        public IEnumerable<FeatureCircle> FeatureCircles =>
            Enumerable.Range(0, Circles.Length).Select(i => new FeatureCircle(this, i));
    };

    public sealed record Node(Features.Node Blueprint, IBiome Biome, ImmutableArray<Circle> Circles)
        : WithCircles(Circles);

    public sealed record Connection(FeatureCircle From, FeatureCircle To, Unit Radius, bool SplitIfPossible = false)
        : PhysicalFeature;

    public sealed record Crevice(ImmutableArray<Circle> Circles)
        : WithCircles(Circles);

    public virtual bool Equals(PhysicalFeature? other) => ReferenceEquals(this, other);

    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode() => base.GetHashCode();
}

readonly struct FeatureCircle : IEquatable<FeatureCircle>
{
    public PhysicalFeature.WithCircles Feature { get; }
    public int Index { get; }

    public Circle Circle => Feature.Circles[Index];

    // TODO: see if there's a type safe way to make sure we always get a correct index
    public FeatureCircle(PhysicalFeature.WithCircles feature, int index)
    {
        if (index < 0 || index >= feature.Circles.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        Feature = feature;
        Index = index;
    }

    public override bool Equals(object? obj) => obj is FeatureCircle other && Equals(other);
    public bool Equals(FeatureCircle other) => Feature.Equals(other.Feature) && Index == other.Index;
    public override int GetHashCode() => HashCode.Combine(Feature, Index);

    public static bool operator ==(FeatureCircle left, FeatureCircle right) => left.Equals(right);
    public static bool operator !=(FeatureCircle left, FeatureCircle right) => !left.Equals(right);
}
