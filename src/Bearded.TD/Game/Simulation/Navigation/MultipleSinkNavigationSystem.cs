using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Navigation;

sealed partial class MultipleSinkNavigationSystem : IListener<TilePassabilityChanged>
{
    private readonly GlobalGameEvents events;
    private readonly Level level;
    private readonly PassabilityLayer passability;
    private readonly Queue<Tile> updateFront = new();
    private readonly HashSet<Tile> updatesInQueue = [];

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

    public Direction GetDirectionToSink(Tile from) => graph[from].Direction;

    public Directions GetAllDirectionsToSink(Tile from)
    {
        var result = Directions.None;
        var fromDistance = graph[from].Distance;
        foreach (var direction in level.ValidDirectionsFrom(from))
        {
            var neighbor = from.Neighbor(direction);
            if (!passability[neighbor].IsPassable)
            {
                continue;
            }

            var neighborDistance = graph[neighbor].Distance;
            // The neighbor is closer to the sink than our current tile, so it's a valid direction to travel in.
            // This distance should only be one fewer than our current distance if the graph is well-formed.
            if (neighborDistance < fromDistance)
            {
                result = result.And(direction);
            }
        }

        // Fallback: use the direction we would pick anyway.
        if (!result.Any())
        {
            result = result.And(GetDirectionToSink(from));
        }

        return result;
    }

    public Direction GetDirectionToClosestToSinkNeighbor(Tile from)
    {
        var minDirection = Direction.Unknown;
        var minDistance = int.MaxValue;
        foreach (var direction in level.ValidDirectionsFrom(from))
        {
            var neighbor = from.Neighbor(direction);
            if (!passability[neighbor].IsPassable)
                continue;
            var node = graph[neighbor];
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
                var neighborTile = tile.Neighbor(direction);
                var neighborNode = graph[neighborTile];
                if (neighborNode.IsInvalid || neighborNode.IsSink)
                    continue;
                if (neighborNode.Direction == direction.Opposite())
                {
                    invalidateTile(neighborTile);
                }
                else
                {
                    touchTile(neighborTile);
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
                var neighborTile = tile.Neighbor(direction);
                var neighborNode = graph[neighborTile];
                if (neighborNode.Distance > newDistance)
                {
                    updateTile(neighborTile, newDistance, direction.Opposite());
                }
                else if (neighborNode.Distance < newDistance
                         && neighborNode.Direction == direction.Opposite())
                {
                    invalidateTile(neighborTile);
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
            var neighbor = tile.Neighbor(direction);
            touchTile(neighbor);
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

    private const int backupSinkDistance = 100_000_000;

    private static Node none => new(int.MaxValue, Direction.Unknown);
    private static Node sink => new(0, Direction.Unknown);
    private static Node backupSink => new(backupSinkDistance, Direction.Unknown);

    private readonly record struct Node(int Distance, Direction Direction)
    {
        public Node Invalidated => this with { Distance = int.MaxValue };

        public bool IsInvalid => Distance == int.MaxValue;
        public bool IsSink => Distance is 0 or backupSinkDistance;
    }
}
