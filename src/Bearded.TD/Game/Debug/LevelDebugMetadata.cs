using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Debug
{
    sealed class LevelDebugMetadata
    {
        public record Data;
        public sealed record LineSegment(Position2 From, Position2 To, Color Color) : Data;
        public sealed record AreaBorder(TileAreaBorder Border, Color Color) : Data;
        public sealed record Tile(Bearded.TD.Tiles.Tile XY, Unit Z, Color Color) : Data;

        private readonly List<Data> data = new();

        public void Add(Position2 from, Position2 to, Color color)
        {
            data.Add(new LineSegment(from, to, color));
        }

        public void Add(TileAreaBorder border, Color color)
        {
            data.Add(new AreaBorder(border, color));
        }

        public void Add(Bearded.TD.Tiles.Tile tile, Color color, Unit z = default)
        {
            data.Add(new Tile(tile, z, color));
        }

        public void Visit(Action<Data> visit)
        {
            // TODO: use a visitor interface or multiple visitor actions for each type for type safety?
            foreach (var d in data)
            {
                visit(d);
            }
        }

        public void Clear()
        {
            data.Clear();
        }
    }
}
