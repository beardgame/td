using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Logical;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

sealed class PhysicalTilemapGenerator
{
    private readonly LevelDebugMetadata metadata;
    private readonly Unit nodeRadius;

    public PhysicalTilemapGenerator(LevelDebugMetadata metadata, Unit nodeRadius)
    {
        this.metadata = metadata;
        this.nodeRadius = nodeRadius;
    }

    public IEnumerable<CommandFactory> Generate(
        LogicalTilemap logicalTilemap, Random random, int radius)
    {
        var featuresWithAreas = new PhysicalFeatureGenerator(nodeRadius)
            .GenerateFeaturesWithAreasInInitialLocation(logicalTilemap, random);

        var arrangedFeaturesWithAreas = new FeatureArranger(radius).ArrangeFeatures(featuresWithAreas);

        var featuresWithTiles = new FeatureTileAssigner(radius).AssignFeatures(arrangedFeaturesWithAreas);

        var (tilemap, levelCommands) = new TilemapGenerator().GenerateTilemap(radius, featuresWithTiles, random);

        var zoneCommand = new ZoneGenerator().GenerateZones(featuresWithTiles, tilemap);

        addFeatureAreaMetadata(arrangedFeaturesWithAreas);
        addFeatureTileMetadata(featuresWithTiles);

        // TODO: these will be generated (and possibly entirely replaced) somewhere above later
        var drawInfos = TileDrawInfo.DrawInfosFromTypes(tilemap);
        CommandFactory tilemapCommand = game => FillTilemap.Command(game, tilemap, drawInfos);

        return ImmutableArray.Create(tilemapCommand, zoneCommand).Concat(levelCommands);
    }

    private void addFeatureTileMetadata(IEnumerable<TiledFeature> features)
    {
        foreach (var feature in features)
        {
            var color = feature.Feature switch
            {
                PhysicalFeature.Connection => Color.Beige,
                PhysicalFeature.Crevice => Color.Brown,
                PhysicalFeature.Node => Color.IndianRed,
                _ => throw new NotSupportedException()
            };

            metadata.Add(new AreaBorder(TileAreaBorder.From(feature.Tiles), color * 0.5f));

            if (feature is TiledFeature.Node node)
            {
                metadata.Add(new AreaBorder(TileAreaBorder.From(node.Connections), Color.Beige * 0.25f));
            }
        }
    }

    private void addFeatureAreaMetadata(IEnumerable<PhysicalFeature> features)
    {
        foreach (var feature in features)
        {
            switch (feature)
            {
                case PhysicalFeature.WithCircles featureWithCircles:
                    foreach (var circle in featureWithCircles.Circles)
                    {
                        metadata.Add(new Circle(circle.Center, circle.Radius, 0.3.U(), Color.Cyan * 0.5f));
                    }
                    if (feature is PhysicalFeature.Node node)
                    {
                        const float lineHeight = 0.5f;
                        var center = Position2.Zero + featureWithCircles.Circles
                                .Aggregate(Difference2.Zero, (p, c) => p + (c.Center - Position2.Zero))
                            / featureWithCircles.Circles.Length;

                        foreach (var (behavior, i) in node.Blueprint.Behaviors.Indexed())
                        {
                            metadata.Add(new Text(
                                center - new Difference2(0, i * lineHeight),
                                behavior.Name,
                                Color.Cyan * 0.5f, 0, lineHeight.U()
                            ));
                        }
                    }
                    break;
                case PhysicalFeature.Connection(var from, var to):
                    metadata.Add(new LineSegment(
                        from.Circle.Center,
                        to.Circle.Center,
                        Color.Azure * 0.5f
                    ));
                    break;
            }
        }
    }
}
