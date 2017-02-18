using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Tilemap;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.Navigation.Directions;

namespace Bearded.TD.Game.Navigation
{
    struct Directions
    {
        public static Directions None => new Directions(int.MaxValue, Direction.Unknown);
        public static Directions Sink => new Directions(0, Direction.Unknown);

        public int Distance { get; }
        public Direction Direction { get; }

        public Directions(int distance, Direction direction)
        {
            Distance = distance;
            Direction = direction;
        }

        public bool IsNone => Distance == int.MaxValue;
        public bool IsSink => Distance == 0;
    }

    struct FrontUpdate
    {
        public int X { get; }
        public int Y { get; }

        public FrontUpdate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class MultipleSinkNavigationSystem
    {
        private readonly Queue<FrontUpdate> updateFront = new Queue<FrontUpdate>();

        private readonly Tilemap<TileInfo> tilemap;
        private readonly Tilemap<Directions> directions;

        public MultipleSinkNavigationSystem(Tilemap<TileInfo> tilemap)
        {
            this.tilemap = tilemap;
            directions = new Tilemap<Directions>(tilemap.Radius);
            foreach (var tile in directions)
                directions[tile] = None;

            AddSink(new Tile<TileInfo>(tilemap, 0, 0));
        }

        public void Update()
        {
            while (updateFront.Count > 0)
            {
                updateOneTile();
                break;
            }
        }

        private void updateOneTile()
        {
            var frontUpdate = updateFront.Dequeue();
            var tile = new Tile<Directions>(directions, frontUpdate.X, frontUpdate.Y);
            var info = tile.Info;

            if (info.IsNone)
            {
                foreach (var direction in Tilemap.Tilemap.Directions)
                {
                    var neighbour = tile.Neighbour(direction);
                    if (!neighbour.IsValid)
                        continue;
                    if (neighbour.Info.Direction == direction.Opposite())
                    {
                        invalidateTile(neighbour);
                    }
                }
            }
            else
            {
                var newDistance = info.Distance + 1;

                foreach (var direction in Tilemap.Tilemap.Directions)
                {
                    var neighbour = tile.Neighbour(direction);
                    if (!neighbour.IsValid)
                        continue;
                    if (neighbour.Info.Distance > newDistance)
                    {
                        updateTile(neighbour, newDistance, direction.Opposite());
                    }
                }
            }
        }

        public void AddSink(Tile<TileInfo> tile)
        {
            updateTile(tile.X, tile.Y, Sink);
        }
        private void invalidateTile(Tile<Directions> tile)
        {
            updateTile(tile.X, tile.Y, None);
        }
        private void updateTile(Tile<Directions> tile, int newDistance, Direction direction)
        {
            updateTile(tile.X, tile.Y, new Directions(newDistance, direction));
        }

        private void updateTile(int x, int y, Directions tileDirections)
        {
            directions[x, y] = tileDirections;
            updateFront.Enqueue(new FrontUpdate(x, y));
        }

        public void DrawDebug(GeometryManager geometries, Level level)
        {
            var geo = geometries.ConsoleBackground;
            var font = geometries.ConsoleFont;

            geo.LineWidth = HexagonSide * 0.05f;
            font.Height = HexagonSide;
            font.Color = Color.Yellow;

            foreach (var tile in directions)
            {
                var info = tile.Info;
                if (info.IsNone)
                    continue;

                var p = level.GetPosition(new Tile<TileInfo>(null, tile.X, tile.Y)).NumericValue;

                var d = info.Direction.Vector() * HexagonWidth;


                if (!info.IsSink)
                {
                    var pointsTo = tile.Neighbour(info.Direction);
                    geo.Color = (pointsTo.Info.IsNone ? Color.Red : Color.Lime) * 0.2f;
                    geo.DrawLine(p, p + d);
                }

                font.DrawString(p, $"{info.Distance}", 0.5f, 0.5f);
            }
        }
    }
}