using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Debug;

sealed class LevelDebugMetadata
{
    public record Data;
    public sealed record LineSegment(Position2 From, Position2 To, Color Color, Unit? Width = null) : Data;
    public sealed record AreaBorder(TileAreaBorder Border, Color Color) : Data;
    public sealed record Tile(Bearded.TD.Tiles.Tile XY, Unit Z, Color Color) : Data;
    public sealed record Circle(Position2 Center, Unit Radius, Unit LineWidth, Color Color) : Data;
    public sealed record Disk(Position2 Center, Unit Radius, Color Color) : Data;

    public sealed record Text(Position2 Position, string Value, Color Color, float? AlignX = null, Unit? FontHeight = null) : Data;

    private readonly List<Data> data = new();

    public void Add(Data data)
    {
        this.data.Add(data);
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
