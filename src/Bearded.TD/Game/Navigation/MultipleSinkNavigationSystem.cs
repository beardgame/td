using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Navigation
{
    class MultipleSinkNavigationSystem
    {
        private readonly Queue<FrontUpdate> updateFront = new Queue<FrontUpdate>();
        private readonly HashSet<FrontUpdate> updatesInQueue = new HashSet<FrontUpdate>();

        private readonly Tilemap<TileInfo> tilemap;
        private readonly Tilemap<Directions> directions;

        public MultipleSinkNavigationSystem(LevelGeometry geometry)
        {
            tilemap = geometry.Tilemap;

            directions = new Tilemap<Directions>(tilemap.Radius);
            foreach (var tile in directions)
                directions[tile] = None;
        }

        public void Initialise(LevelGeometry geometry)
        {
            geometry.TilePassabilityChanged += tilePassabilityChanged;
        }

        public Direction GetDirections(Tile<TileInfo> from) => directions[from.X, from.Y].Direction;

        private void tilePassabilityChanged(Tile<TileInfo> tile)
        {
            if (directions[tile.X, tile.Y].IsSink)
                return;

            invalidateTile(tile.X, tile.Y);
        }

        public void Update()
        {
            for (var i = 0; i < 100; i++)
            {
                if (updateFront.Count == 0)
                    break;
                 updateOneTile();
            }
        }

        private void updateOneTile()
        {
            var xy = updateFront.Dequeue();
            updatesInQueue.Remove(xy);
            var tile = new Tile<Directions>(directions, xy.X, xy.Y);
            var info = tile.Info;

            if (info.IsInvalid)
            {
                foreach (var direction in Tilemap.Directions)
                {
                    var neighbour = tile.Neighbour(direction);
                    if (!neighbour.IsValid)
                        continue;
                    var neighbourInfo = neighbour.Info;
                    if (neighbourInfo.IsInvalid)
                        continue;
                    if (neighbourInfo.Direction == direction.Opposite())
                    {
                        invalidateTile(neighbour);
                    }
                    else
                    {
                        touchTile(neighbour.X, neighbour.Y);
                    }
                }
            }
            else
            {
                if (!info.IsSink && tile.Neighbour(info.Direction).Info.IsInvalid)
                {
                    invalidateTile(tile);
                    return;
                }

                var newDistance = info.Distance + 1;

                var gameTile = new Tile<TileInfo>(tilemap, xy.X, xy.Y);

                foreach (var direction in gameTile.Info.OpenDirections.Enumerate())

                {
                    var neighbour = tile.Neighbour(direction);
                    var neighbourInfo = neighbour.Info;
                    if (neighbourInfo.Distance > newDistance)
                    {
                        updateTile(neighbour, newDistance, direction.Opposite());
                    }
                    else if (neighbourInfo.Distance < newDistance)
                    {
                        if (neighbourInfo.Direction == direction.Opposite())
                        {
                            invalidateTile(neighbour);
                        }
                    }
                }
            }
        }

        public void AddSink(Tile<TileInfo> tile)
        {
            updateTile(tile.X, tile.Y, Sink);
        }
        public void RemoveSink(Tile<TileInfo> tile)
        {
            invalidateTile(tile.X, tile.Y);
        }

        private void invalidateTile(Tile<Directions> tile)
        {
            invalidateTile(tile.X, tile.Y);
        }
        private void invalidateTile(int x, int y)
        {
            updateTile(x, y, directions[x, y].Invalidated);
        }
        private void updateTile(Tile<Directions> tile, int newDistance, Direction direction)
        {
            updateTile(tile.X, tile.Y, new Directions(newDistance, direction));
        }

        private void updateTile(int x, int y, Directions tileDirections)
        {
            directions[x, y] = tileDirections;
            touchTile(x, y);
        }
        private void touchTile(int x, int y)
        {
            var update = new FrontUpdate(x, y);
            if (!updatesInQueue.Add(update))
                return;
            updateFront.Enqueue(update);
        }

        public void DrawDebug(GeometryManager geometries, Level level)
        {
            var geo = geometries.ConsoleBackground;
            var font = geometries.ConsoleFont;

            geo.LineWidth = HexagonSide * 0.05f;
            geo.Color = Color.Orange * 0.3f;

            font.Height = HexagonSide;
            font.Color = Color.Orange;

            const float w = HexagonDistanceX * 0.5f - 0.1f;
            const float h = HexagonDistanceY * 0.5f - 0.1f;

            var i = 0;
            foreach (var tile in updateFront)
            {
                var p = level.GetPosition(new Tile<TileInfo>(null, tile.X, tile.Y)).NumericValue;

                geo.DrawRectangle(p.X - w, p.Y - h, w * 2, h * 2);

                font.DrawString(p, $"{i}", 0.5f, 1f);
                i++;
            }

            font.Height = HexagonSide;
            font.Color = Color.Yellow;

            foreach (var tile in directions)
            {
                var info = tile.Info;

                var p = level.GetPosition(new Tile<TileInfo>(null, tile.X, tile.Y)).NumericValue;

                var d = info.Direction.Vector() * HexagonWidth;


                if (!info.IsSink)
                {
                    var pointsTo = tile.Neighbour(info.Direction);

                    if (!pointsTo.Info.IsInvalid && pointsTo.Info.Distance >= info.Distance)
                    {

                        geo.Color = Color.Red * 0.3f;
                        geo.DrawRectangle(p.X - w, p.Y - h, w * 2, h * 2);
                    }

                    geo.Color = (pointsTo.Info.IsInvalid ? Color.Red : Color.DarkGreen) * 0.8f;
                    geo.DrawLine(p, p + d);
                }

                if (!info.IsInvalid)
                    font.DrawString(p, $"{info.Distance}", 0.5f);
            }
        }


        private static Directions None => new Directions(int.MaxValue, Direction.Unknown);
        private static Directions Sink => new Directions(0, Direction.Unknown);

        private struct Directions
        {
            public int Distance { get; }
            public Direction Direction { get; }

            public Directions(int distance, Direction direction)
            {
                Distance = distance;
                Direction = direction;
            }

            public Directions Invalidated => new Directions(int.MaxValue, Direction);

            public bool IsInvalid => Distance == int.MaxValue;
            public bool IsSink => Distance == 0;
        }

        private struct FrontUpdate : IEquatable<FrontUpdate>
        {
            public int X { get; }
            public int Y { get; }

            public FrontUpdate(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Equals(FrontUpdate other) => X == other.X && Y == other.Y;

            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj) && (obj is FrontUpdate && Equals((FrontUpdate) obj));

            public override int GetHashCode() => (X * 397) ^ Y;

        }
    }
}
