using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    abstract record PhysicalFeature
    {
        private PhysicalFeature()
        {
        }

        public TiledFeature WithTiles(IEnumerable<Tile> tiles)
            => this switch
            {
                Connection c => TiledFeature.From(c, tiles),
                Crevice c => TiledFeature.From(c, tiles),
                Node n => TiledFeature.From(n, tiles),
                _ => throw new NotImplementedException()
            };

        public sealed record Node(Features.Node Blueprint, ImmutableArray<RelaxationCircle> Circles)
            : PhysicalFeature, IFeatureWithCircles;

        public sealed record Connection(NodeCircle From, NodeCircle To)
            : PhysicalFeature;

        public sealed record Crevice(ImmutableArray<RelaxationCircle> Circles)
            : PhysicalFeature, IFeatureWithCircles;

        public virtual bool Equals(PhysicalFeature? other) => ReferenceEquals(this, other);

        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();
    }

    interface IFeatureWithCircles
    {
        // TODO: this should be using an immutable circle type - the mutability should be container to the system that needs it
        public ImmutableArray<RelaxationCircle> Circles { get; }
        TiledFeature WithTiles(IEnumerable<Tile> tiles);
    }

    sealed record NodeCircle
    {
        public PhysicalFeature.Node Node { get; }
        private readonly int index;

        public RelaxationCircle Circle => Node.Circles[index];

        public NodeCircle(PhysicalFeature.Node node, int index)
        {
            if (index < 0 || index >= node.Circles.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            Node = node;
            this.index = index;
        }
    }
}
