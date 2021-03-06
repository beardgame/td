using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.Linq;
using static Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout.PhysicalFeature;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class FeatureTileAssigner
    {
        private readonly int tilemapRadius;

        public FeatureTileAssigner(int tilemapRadius)
        {
            this.tilemapRadius = tilemapRadius;
        }

        public List<TiledFeature> AssignFeatures(IEnumerable<PhysicalFeature> features)
        {
            var featureList = features as ICollection<PhysicalFeature> ?? features.ToList();
            var nodes = featureList.OfType<Node>().ToList();
            var crevices = featureList.OfType<Crevice>().ToList();
            var connections = featureList.OfType<Connection>().ToList();

            var nodesWithAreas = assignTilesToClosestContainingCircleArea(nodes);
            var nodesWithAreasAfterErosion = erode(nodesWithAreas).ToList();
            var nodeTiles = nodesWithAreasAfterErosion.SelectMany(f => f.Tiles.Enumerated).ToImmutableHashSet();
            var crevicesWithAreas = assignTilesToClosestContainingCircleArea(crevices, nodeTiles);
            var (connectionsWithAreas, connectionBoundaryTiles) = assignTilesAlongConnection(connections, nodeTiles);

            var nodesWithAreasAndConnectionTiles = nodesWithAreasAfterErosion
                .Select(n => new TiledFeature.Node((Node) n.Feature, n.Tiles,
                    connectionBoundaryTiles.Where(n.Tiles.Contains).ToImmutableArray()));

            return nodesWithAreasAndConnectionTiles
                .Concat(crevicesWithAreas)
                .Concat(connectionsWithAreas)
                .ToList();
        }


        private IEnumerable<TiledFeature> assignTilesToClosestContainingCircleArea(
            IReadOnlyCollection<WithCircles> features, IEnumerable<Tile>? tilesToAvoid = null)
        {
            // TODO: 🐎 refactor to iterate tiles around circles instead and keep a tilemap with closest node for each tile
            // we may need to use a shared tilemap with other features for this so connections can assign tiles to nodes,
            // and similar
            var featureAreas = features.ToDictionary(f => f, _ => new HashSet<Tile>());
            var allCircles = features
                .SelectMany(f => f.Circles.Select(c => (Circle: c, Feature: f)))
                .ToList();

            var avoid = (ISet<Tile>)(tilesToAvoid as HashSet<Tile>) ??
                ImmutableHashSet.CreateRange(tilesToAvoid ?? ImmutableHashSet<Tile>.Empty);

            foreach (var tile in Tilemap.EnumerateTilemapWith(tilemapRadius))
            {
                if (avoid.Contains(tile))
                    continue;

                var tilePosition = Level.GetPosition(tile);

                var (circle, feature) = allCircles.MinBy(
                    c => ((tilePosition - c.Circle.Center).Length - c.Circle.Radius).NumericValue);

                var distanceToNodeSquared = (tilePosition - circle.Center).LengthSquared;

                if (distanceToNodeSquared > circle.Radius.Squared)
                    continue;

                featureAreas[feature].Add(tile);
            }

            return features.Select(f => f.WithTiles(featureAreas[f]));
        }

        private (List<TiledFeature> ConnectionFeatures, List<Tile> ConnectionBoundaryTiles)
            assignTilesAlongConnection(
            IEnumerable<Connection> features, IEnumerable<Tile>? tilesToAvoid = null)
        {
            var avoid = (ISet<Tile>)(tilesToAvoid as HashSet<Tile>) ??
                ImmutableHashSet.CreateRange(tilesToAvoid ?? ImmutableHashSet<Tile>.Empty);

            var featuresWithTiles = new List<TiledFeature>();
            var boundaryTiles = new List<Tile>();

            foreach (var feature in features)
            {
                var (from, to) = feature;

                var rayCaster = new LevelRayCaster();
                rayCaster.StartEnumeratingTiles(new Ray(from.Circle.Center, to.Circle.Center));

                var featureArea = new List<Tile>();
                var adding = false;
                Tile? previousTile = null;

                foreach (var tile in rayCaster)
                {
                    // TODO: this is a bit ugly, and we can maybe ensure better that we are actually connecting..
                    // the nodes we mean to connect?
                    if (avoid.Contains(tile))
                    {
                        // abort when we reach target
                        if (adding)
                        {
                            if (previousTile.HasValue)
                                boundaryTiles.Add(previousTile.Value);
                            boundaryTiles.Add(tile);
                            break;
                        }
                        // skip until the first tile we can take
                        previousTile = tile;
                        continue;
                    }

                    adding = true;

                    featureArea.Add(tile);
                }
                featuresWithTiles.Add(feature.WithTiles(featureArea));
            }

            return (featuresWithTiles, boundaryTiles);
        }

        private IEnumerable<TiledFeature> erode(IEnumerable<TiledFeature> features)
        {
            return features
                .Select(f => f.Feature.WithTiles(f.Tiles.Enumerated.Where(t => t.PossibleNeighbours().All(f.Tiles.Contains))));
        }
    }
}
