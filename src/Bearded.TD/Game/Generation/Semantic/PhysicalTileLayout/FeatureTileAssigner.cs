using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class FeatureTileAssigner
    {
        private readonly int tilemapRadius;

        public FeatureTileAssigner(int tilemapRadius)
        {
            this.tilemapRadius = tilemapRadius;
        }

        public List<FeatureWithTiles> AssignFeatures(IEnumerable<IFeatureWithArea> features)
        {
            var byType = features.ToLookup(f => f.Feature.GetType());
            var nodes = byType[typeof(NodeFeature)].ToList();
            var crevices = byType[typeof(CreviceFeature)].ToList();
            var connections = byType[typeof(ConnectionFeature)];

            var nodesWithAreas = assignTilesToClosestContainingCircleArea(nodes);
            var nodesWithAreasAfterErosion = erode(nodesWithAreas).ToList();
            var nodeTiles = nodesWithAreasAfterErosion.SelectMany(f => f.Tiles).ToImmutableHashSet();
            var crevicesWithAreas = assignTilesToClosestContainingCircleArea(crevices, nodeTiles);
            var connectionsWithAreas = assignTilesAlongLingSegments(connections, nodeTiles);

            return nodesWithAreasAfterErosion
                .Concat(crevicesWithAreas)
                .Concat(connectionsWithAreas)
                .ToList();
        }


        private IEnumerable<FeatureWithTiles> assignTilesToClosestContainingCircleArea(
            ICollection<IFeatureWithArea> features, IEnumerable<Tile>? tilesToAvoid = null)
        {
            // TODO: 🐎 refactor to iterate tiles around circles instead and keep a tilemap with closest node for each tile
            // we may need to use a shared tilemap with other features for this so connections can assign tiles to nodes,
            // and similar
            var featureAreas = features.ToDictionary(f => f, _ => new HashSet<Tile>());
            var allCircles = features
                .SelectMany(f => ((CirclesArea) f.Area).Circles.Select(c => (Circle: c, Feature: f)))
                .ToList();

            var avoid = (ISet<Tile>)(tilesToAvoid as HashSet<Tile>) ??
                ImmutableHashSet.CreateRange(tilesToAvoid ?? ImmutableHashSet<Tile>.Empty);

            foreach (var tile in Tilemap.EnumerateTilemapWith(tilemapRadius))
            {
                if (avoid.Contains(tile))
                    continue;

                var tilePosition = Level.GetPosition(tile);

                var (circle, feature) = allCircles.MinBy(
                    c => ((tilePosition - c.Circle.Position).Length - c.Circle.Radius).NumericValue);

                var distanceToNodeSquared = (tilePosition - circle.Position).LengthSquared;

                if (distanceToNodeSquared > circle.Radius.Squared)
                    continue;

                featureAreas[feature].Add(tile);
            }

            return features.Select(f => f.Feature.WithTiles(featureAreas[f]));
        }

        private IEnumerable<FeatureWithTiles> assignTilesAlongLingSegments(
            IEnumerable<IFeatureWithArea> features, IEnumerable<Tile>? tilesToAvoid = null)
        {
            var avoid = (ISet<Tile>)(tilesToAvoid as HashSet<Tile>) ??
                ImmutableHashSet.CreateRange(tilesToAvoid ?? ImmutableHashSet<Tile>.Empty);

            foreach (var feature in features)
            {
                var (from, to) = (LineSegmentArea)feature.Area;

                var rayCaster = new LevelRayCaster();
                rayCaster.StartEnumeratingTiles(new Ray(from.Position, to.Position - from.Position));

                var featureArea = new List<Tile>();
                var adding = false;

                foreach (var tile in rayCaster)
                {
                    // TODO: this is a bit ugly, and we can maybe ensure better that we are actually connecting..
                    // the nodes we mean to connect?
                    if (avoid.Contains(tile))
                    {
                        // abort when we reach target
                        if (adding)
                            break;
                        // skip until the first tile we can take
                        continue;
                    }

                    adding = true;

                    featureArea.Add(tile);
                }

                yield return feature.Feature.WithTiles(featureArea);
            }
        }

        private IEnumerable<FeatureWithTiles> erode(IEnumerable<FeatureWithTiles> features)
        {
            return features
                .Select(f => f.Feature.WithTiles(f.Tiles.Where(t => t.PossibleNeighbours().All(f.Tiles.Contains))));
        }
    }
}
