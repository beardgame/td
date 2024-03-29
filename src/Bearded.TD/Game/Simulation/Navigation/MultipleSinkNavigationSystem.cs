﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Simulation.Navigation;

sealed class MultipleSinkNavigationSystem : IListener<TilePassabilityChanged>
{
    private readonly GlobalGameEvents events;
    private readonly Level level;
    private readonly PassabilityLayer passability;
    private readonly Queue<Tile> updateFront = new Queue<Tile>();
    private readonly HashSet<Tile> updatesInQueue = new HashSet<Tile>();

    private readonly Tilemap<Node> graph;

    public MultipleSinkNavigationSystem(GlobalGameEvents events, Level level, PassabilityLayer passability)
    {
        this.events = events;
        this.level = level;
        this.passability = passability;

        graph = new Tilemap<Node>(level.Radius);
        foreach (var tile in graph)
            graph[tile] = none;
    }

    public void Initialize()
    {
        events.Subscribe(this);
    }

    public Direction GetDirections(Tile from) => graph[from].Direction;

    public Direction GetDirectionToClosestToSinkNeighbour(Tile from)
    {
        var minDirection = Direction.Unknown;
        var minDistance = int.MaxValue;
        foreach (var direction in level.ValidDirectionsFrom(from))
        {
            var neighbour = from.Neighbor(direction);
            if (!passability[neighbour].IsPassable)
                continue;
            var node = graph[neighbour];
            if (node.Distance >= minDistance)
                continue;
            minDirection = direction;
            minDistance = node.Distance;
        }

        return minDirection;
    }

    public int GetDistanceToClosestSink(Tile tile)
    {
        var distance = graph[tile].Distance;
        return distance < backupSinkDistance
            ? distance
            : distance - backupSinkDistance;
    }

    public void HandleEvent(TilePassabilityChanged @event)
    {
        var tile = @event.Tile;

        invalidateTile(tile);
    }

    public void Update()
    {
        for (var i = 0; i < Constants.Game.Navigation.StepsPerFrame; i++)
        {
            if (updateFront.Count == 0)
                break;
            updateOneTile();
        }
    }

    private void updateOneTile()
    {
        var tile = updateFront.Dequeue();
        updatesInQueue.Remove(tile);
        var node = graph[tile];

        if (node.IsInvalid)
        {
            foreach (var direction in level.ValidDirectionsFrom(tile))
            {
                var neighbourTile = tile.Neighbor(direction);
                var neighbourNode = graph[neighbourTile];
                if (neighbourNode.IsInvalid || neighbourNode.IsSink)
                    continue;
                if (neighbourNode.Direction == direction.Opposite())
                {
                    invalidateTile(neighbourTile);
                }
                else
                {
                    touchTile(neighbourTile);
                }
            }
        }
        else
        {
            if (!node.IsSink && graph[tile.Neighbor(node.Direction)].IsInvalid)
            {
                invalidateTile(tile);
                return;
            }

            var newDistance = node.Distance + 1;

            var invalidatedAChild = false;

            foreach (var direction in passableDirectionsFrom(tile).Enumerate())
            {
                var neighbourTile = tile.Neighbor(direction);
                var neighbourNode = graph[neighbourTile];
                if (neighbourNode.Distance > newDistance)
                {
                    updateTile(neighbourTile, newDistance, direction.Opposite());
                }
                else if (neighbourNode.Distance < newDistance
                         && neighbourNode.Direction == direction.Opposite())
                {
                    invalidateTile(neighbourTile);
                    invalidatedAChild = true;
                }
            }

            if (invalidatedAChild)
            {
                touchTile(tile);
            }
        }
    }

    public void AddSink(Tile tile)
    {
        updateTile(tile, sink);
    }
    public void AddBackupSink(Tile tile)
    {
        updateTile(tile, backupSink);
        foreach (var direction in passableDirectionsFrom(tile).Enumerate())
        {
            var neighbour = tile.Neighbor(direction);
            touchTile(neighbour);
        }
    }
    public void RemoveSink(Tile tile)
    {
        invalidateTile(tile);
    }

    private void invalidateTile(Tile tile)
    {
        updateTile(tile, graph[tile].Invalidated);
    }
    private void updateTile(Tile tile, int newDistance, Direction direction)
    {
        updateTile(tile, new Node(newDistance, direction));
    }
    private void updateTile(Tile tile, Node node)
    {
        graph[tile] = node;
        touchTile(tile);
    }
    private void touchTile(Tile tile)
    {
        if (!updatesInQueue.Add(tile))
            return;
        updateFront.Enqueue(tile);
    }

    private Directions passableDirectionsFrom(Tile tile)
    {
        return passability[tile].PassableDirections;
    }

    public void DrawDebug(CoreDrawers drawers, bool drawWeights)
    {
        var shapeDrawer = drawers.ConsoleBackground;
        var textDrawer = drawers.InGameConsoleFont;

        const float lineWidth = HexagonSide * 0.05f;
        const float fontHeight = HexagonSide;

        var weightsFontColor = Color.Orange;

        const float w = HexagonDistanceX * 0.5f - 0.1f;
        const float h = HexagonDistanceY * 0.5f - 0.1f;

        if (drawWeights)
        {
            var i = 0;
            foreach (var tile in updateFront)
            {
                var p = Level.GetPosition(tile).NumericValue;

                shapeDrawer.FillRectangle(p.X - w, p.Y - h, w * 2, h * 2, weightsFontColor * 0.3f);

                textDrawer.DrawLine(
                    xyz: p.WithZ(),
                    text: $"{i}",
                    fontHeight: fontHeight,
                    alignHorizontal: 0.5f,
                    alignVertical: 1f,
                    parameters: weightsFontColor);
                i++;
            }
        }

        foreach (var tile in graph)
        {
            var node = graph[tile];

            var p = Level.GetPosition(tile).NumericValue;

            var d = node.Direction.Vector() * HexagonWidth;

            if (!node.IsSink)
            {
                var pointsToTile = tile.Neighbor(node.Direction);
                var pointsToNode = graph[pointsToTile];

                if (!pointsToNode.IsInvalid && pointsToNode.Distance >= node.Distance)
                {
                    shapeDrawer.FillRectangle(p.X - w, p.Y - h, w * 2, h * 2, Color.Red * 0.3f);
                }

                var color = (pointsToNode.IsInvalid ? Color.Red : Color.DarkGreen) * 0.8f;
                shapeDrawer.DrawLine(p, p + d, lineWidth, color);
                shapeDrawer.DrawLine(p + d, p + .9f * d + .1f * d.PerpendicularRight, lineWidth, color);
                shapeDrawer.DrawLine(p + d, p + .9f * d + .1f * d.PerpendicularLeft, lineWidth, color);
            }

            if (drawWeights && !node.IsInvalid)
            {
                var color = Color.Yellow;
                var distance = node.Distance;
                if (distance >= backupSinkDistance)
                {
                    distance -= backupSinkDistance;
                    color = Color.Red;
                }

                if (!node.IsSink && level.ValidNeighboursOf(tile).Select(t => graph[t])
                        .Any(n => !n.IsInvalid && !n.IsSink && Math.Abs(n.Distance - node.Distance) > 1))
                {
                    color = Color.MediumPurple;
                }

                textDrawer.DrawLine(
                    xyz: p.WithZ(),
                    text: $"{distance}",
                    fontHeight: fontHeight,
                    alignHorizontal: 0.5f,
                    parameters: color);
            }
        }
    }

    private const int backupSinkDistance = 100_000_000;

    private static Node none => new Node(int.MaxValue, Direction.Unknown);
    private static Node sink => new Node(0, Direction.Unknown);
    private static Node backupSink => new Node(backupSinkDistance, Direction.Unknown);

    private readonly struct Node
    {
        public int Distance { get; }
        public Direction Direction { get; }

        public Node(int distance, Direction direction)
        {
            Distance = distance;
            Direction = direction;
        }

        public Node Invalidated => new Node(int.MaxValue, Direction);

        public bool IsInvalid => Distance == int.MaxValue;
        public bool IsSink => Distance == 0 || Distance == backupSinkDistance;
    }
}