using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout.PhysicalFeature;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

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
        var nodesByTile = nodesWithAreasAfterErosion.SelectMany(f => f.Tiles.Select(t => (Tile: t, Node: f)))
            .ToImmutableDictionary(t => t.Tile, t => t.Node);
        var nodeTiles = nodesByTile.Keys.ToImmutableHashSet();
        var crevicesWithAreas = assignTilesToClosestContainingCircleArea(crevices, nodeTiles);
        var (connectionsWithAreas, connectionBoundaryTiles) =
            assignTilesAlongConnection(connections, nodeTiles, nodesByTile);

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
            IEnumerable<Connection> features, IEnumerable<Tile> tilesToAvoid,
            IDictionary<Tile, TiledFeature> nodesByTile)
    {
        var avoid = (ISet<Tile>)(tilesToAvoid as HashSet<Tile>) ?? ImmutableHashSet.CreateRange(tilesToAvoid);

        var featuresWithTiles = new List<TiledFeature>();
        var boundaryTiles = new List<Tile>();

        foreach (var feature in features)
        {
            var (from, to, _) = feature;

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
                        DebugAssert.State.Satisfies(nodesByTile[tile].Feature == to.Feature);
                        if (previousTile.HasValue)
                            boundaryTiles.Add(previousTile.Value);
                        boundaryTiles.Add(tile);
                        break;
                    }
                    // skip until the first tile we can take
                    DebugAssert.State.Satisfies(nodesByTile[tile].Feature == from.Feature);
                    previousTile = tile;
                    continue;
                }

                adding = true;

                featureArea.Add(tile);
            }

            var dilutedArea = diluteConnection(featureArea, feature, nodesByTile);

            featuresWithTiles.Add(feature.WithTiles(dilutedArea));
        }

        return (featuresWithTiles, boundaryTiles);
    }

    private IEnumerable<Tile> diluteConnection(
        List<Tile> featureArea, Connection feature, IDictionary<Tile, TiledFeature> nodesByTile)
    {
        var (feature1, feature2) = (feature.From.Feature, feature.To.Feature);
        var (point1, point2) = (feature.From.Circle.Center.NumericValue, feature.To.Circle.Center.NumericValue);
        var connectionVector = point2 - point1;
        var connectionLength = connectionVector.Length;
        var connectionVectorNormalized = connectionVector / connectionLength;

        var tilesToDilute = new Queue<Tile>();
        featureArea.ForEach(tilesToDilute.Enqueue);

        var dilutedArea = new HashSet<Tile>(featureArea);

        while (tilesToDilute.Count > 0)
        {
            var tile = tilesToDilute.Dequeue();

            foreach (var neighbor in tile.PossibleNeighbours().Where(n => n.Radius < tilemapRadius))
            {
                // already seen tile
                if (dilutedArea.Contains(neighbor))
                    continue;

                // never expand into any node
                if (belongsToAnyNode(neighbor))
                    continue;

                // don't touch any other than the connecting nodes
                if (neighbor.PossibleNeighbours().Any(belongsToOtherNode))
                    continue;

                // don't dilute past connection radius
                if (distanceToConnection(neighbor) > feature.Radius)
                    continue;

                // don't extend connection past it's ends
                if (scalarProjectionOf(tile) is < 0 or > 1)
                    continue;

                dilutedArea.Add(neighbor);
                tilesToDilute.Enqueue(neighbor);
            }
        }
        Unit distanceToConnection(Tile t)
        {
            var p = Level.GetPosition(t).NumericValue;

            // source: https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points
            var a = Math.Abs(connectionVector.X * (point1.Y - p.Y) - connectionVector.Y * (point1.X - p.X));
            return (a / connectionLength).U();
        }

        float scalarProjectionOf(Tile t)
        {
            var p = Level.GetPosition(t).NumericValue;
            var scalar = Vector2.Dot(p - point1, connectionVectorNormalized);
            return scalar / connectionLength;
        }

        bool belongsToAnyNode(Tile t) => nodesByTile.ContainsKey(t);

        bool belongsToOtherNode(Tile t)
            => nodesByTile.TryGetValue(t, out var node) && node.Feature != feature1 && node.Feature != feature2;

        return dilutedArea;
    }

    private IEnumerable<TiledFeature> erode(IEnumerable<TiledFeature> features)
    {
        return features
            .Select(f => f.Feature.WithTiles(f.Tiles.Where(t => t.PossibleNeighbours().All(f.Tiles.Contains))));
    }
}
