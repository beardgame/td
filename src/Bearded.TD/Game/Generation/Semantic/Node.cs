using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using Extensions = Bearded.TD.Tiles.Extensions;
using Tile = Bearded.TD.Tiles.Tile;

namespace Bearded.TD.Game.Generation.Semantic
{

    record NodeBlueprint(ImmutableArray<INodeBehavior> Behaviors)
    {
        private ImmutableHashSet<NodeTag>? tags;

        public ImmutableHashSet<NodeTag> Tags => tags ??= Behaviors.SelectMany(b => b.Tags).ToImmutableHashSet();
    }

    interface INodeBehavior
    {
        double GetFitnessPenalty(LogicalTilemap tilemap, Tile nodeTile) => 0;

        IEnumerable<NodeTag> Tags => Enumerable.Empty<NodeTag>();

        void Generate(NodeGenerationContext context)
        {
        }

        // TODO: maybe wanna add ImmutableArray<string> Tags { get; } (not 'string' though)

        string Name => Regex.Replace(GetType().Name, "(Node)?(Behaviou?r)?$", "");
    }

    record NodeTag(string Name);

    class BaseNodeBehavior : INodeBehavior
    {
        public IEnumerable<NodeTag> Tags { get; } = ImmutableArray.Create(new NodeTag("base"));
    }

    class ForceToCenter : INodeBehavior
    {
        public double GetFitnessPenalty(LogicalTilemap tilemap, Tile nodeTile)
        {
            return nodeTile.Radius * 1000;
        }
    }

    class SpawnerNodeBehavior : INodeBehavior
    {
        public IEnumerable<NodeTag> Tags { get; } = ImmutableArray.Create(new NodeTag("spawner"));
    }

    class DontBeAdjacentToTag : INodeBehavior
    {
        private readonly NodeTag tagToAvoid;

        public DontBeAdjacentToTag(NodeTag tagToAvoid)
        {
            this.tagToAvoid = tagToAvoid;
        }

        public double GetFitnessPenalty(LogicalTilemap tilemap, Tile nodeTile)
        {
            var node = tilemap[nodeTile];

            var connectedNodes = Extensions.Directions
                .Where(d => node!.ConnectedTo.Includes(d))
                .Select(nodeTile.Neighbour)
                .Select(t => tilemap[t]);

            return connectedNodes.Count(n => n.Blueprint!.Tags.Contains(tagToAvoid)) * 100;
        }
    }

    class ClearAllTiles : INodeBehavior
    {
        public void Generate(NodeGenerationContext context)
        {
            foreach (var t in context.Tiles)
            {
                context.Set(t, new TileGeometry(TileType.Floor, 0, 0.U()));
            }
        }
    }

    class NodeGenerationContext
    {
        private readonly Tilemap<TileGeometry> tilemap;

        public ImmutableHashSet<Tile> Tiles { get; }

        // info about connections, etc.

        public NodeGenerationContext(Tilemap<TileGeometry> tilemap, IEnumerable<Tile> tiles)
        {
            this.tilemap = tilemap;
            Tiles = tiles.ToImmutableHashSet();
        }

        public void Set(Tile tile, TileGeometry geometry)
        {
            if (!Tiles.Contains(tile))
                throw new ArgumentException("May not write to tile outside node.", nameof(tile));

            tilemap[tile] = geometry;
        }
    }

}
