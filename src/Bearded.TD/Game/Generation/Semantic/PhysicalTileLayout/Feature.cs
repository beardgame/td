using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Logical;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    abstract record Feature
    {
        public abstract void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles);

        public FeatureWithTiles WithTiles(IEnumerable<Tile> tiles) => new(tiles.ToImmutableHashSet(), this);

        public FeatureWithArea<TArea> WithArea<TArea>(TArea area)
            where TArea : Area
            => new(this, area);

        public virtual bool Equals(Feature? other) => ReferenceEquals(this, other);
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();
    }
     sealed record FeatureWithTiles(ImmutableHashSet<Tile> Tiles, Feature Feature)
    {
        public void GenerateTiles(Tilemap<TileGeometry> tilemap) => Feature.GenerateTiles(tilemap, Tiles);
    }

    sealed record NodeFeature(PlacedNode Node) : Feature
    {
        public override void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles)
        {
            var context = new NodeGenerationContext(tilemap, tiles);
            foreach (var behavior in Node.Blueprint!.Behaviors)
            {
                behavior.Generate(context);
            }
        }
    }

    sealed record CreviceFeature : Feature
    {
        public override void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                tilemap[tile] = new TileGeometry(TileType.Crevice, 1, -5.U());
            }
        }
    }

    sealed record ConnectionFeature : Feature
    {
        public override void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                tilemap[tile] = new TileGeometry(TileType.Floor, 1, Unit.Zero);
            }
        }
    }

    interface IFeatureWithArea
    {
        Feature Feature { get; }
        Area Area { get; }
    }

    sealed record FeatureWithArea<TArea>(Feature Feature, TArea Area) : IFeatureWithArea
        where TArea : Area
    {
        Area IFeatureWithArea.Area => Area;
    }

    abstract record Area;

    sealed record CirclesArea(ImmutableArray<RelaxationCircle> Circles) : Area
    {
        public CirclesArea(params RelaxationCircle[] circles) : this(circles.ToImmutableArray())
        {
        }

        public CirclesArea(IEnumerable<RelaxationCircle> circles) : this(circles.ToImmutableArray())
        {
        }
    };

    sealed record LineSegmentArea(RelaxationCircle From, RelaxationCircle To) : Area;

}
