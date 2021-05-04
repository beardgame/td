using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation.Semantic.Logical;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class PhysicalTilemapGenerator
    {
        private readonly LevelDebugMetadata metadata;
        private readonly Unit nodeRadius;

        public PhysicalTilemapGenerator(LevelDebugMetadata metadata, Unit nodeRadius)
        {
            this.metadata = metadata;
            this.nodeRadius = nodeRadius;
        }

        public Tilemap<TileGeometry> Generate(LogicalTilemap logicalTilemap, Random random, int radius)
        {
            var featuresWithAreas = new PhysicalFeatureGenerator(nodeRadius)
                .GenerateFeaturesWithAreasInInitialLocation(logicalTilemap, random);

            new FeatureArranger(radius).ArrangeFeatures(featuresWithAreas);

            var featuresWithTiles = new FeatureTileAssigner(radius).AssignFeatures(featuresWithAreas);

            var finalTilemap = generateFinalTilemap(radius, featuresWithTiles);

            addFeatureAreaMetadata(featuresWithAreas);
            addFeatureTileMetadata(featuresWithTiles);

            return finalTilemap;
        }

        private static Tilemap<TileGeometry> generateFinalTilemap(int radius, IEnumerable<FeatureWithTiles> features)
        {
            var tilemap = new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Wall, 1, Unit.Zero));

            foreach (var feature in features)
            {
                feature.GenerateTiles(tilemap);
            }

            return tilemap;
        }

        private void addFeatureTileMetadata(IEnumerable<FeatureWithTiles> features)
        {
            foreach (var feature in features)
            {
                var color = feature.Feature switch
                {
                    ConnectionFeature => Color.Beige,
                    CreviceFeature => Color.Brown,
                    NodeFeature => Color.IndianRed,
                    _ => throw new ArgumentOutOfRangeException(nameof(feature))
                };

                metadata.Add(new AreaBorder(TileAreaBorder.From(feature.Tiles), color * 0.5f));
            }
        }

        private void addFeatureAreaMetadata(IEnumerable<IFeatureWithArea> features)
        {
            foreach (var feature in features)
            {
                switch (feature.Area)
                {
                    case CirclesArea(var circles):
                        foreach (var circle in circles)
                        {
                            metadata.Add(new Circle(circle.Position, circle.Radius, 0.3.U(), Color.Cyan * 0.5f));
                        }

                        if (feature.Feature is NodeFeature node)
                        {
                            const float lineHeight = 0.5f;
                            var center = Position2.Zero + circles
                                    .Aggregate(Difference2.Zero, (p, c) => p + (c.Position - Position2.Zero))
                                / circles.Length;

                            foreach (var (behavior, i) in node.Node.Blueprint!.Behaviors.Indexed())
                            {
                                metadata.Add(new Text(
                                    center - new Difference2(0, i * lineHeight),
                                    behavior.Name,
                                    Color.Cyan * 0.5f, 0, lineHeight.U()
                                ));
                            }
                        }

                        break;
                    case LineSegmentArea(var from, var to):
                        metadata.Add(new LineSegment(
                            from.Position,
                            to.Position,
                            Color.Azure * 0.5f
                        ));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
