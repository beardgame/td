using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Drawing;

sealed class TileRangeDrawer
{
    private readonly GameState game;
    private readonly Func<RangeDrawStyle> getDrawStyle;
    private readonly Func<TileAreaBorder> getTilesInRange;
    private readonly Color color;

    public TileRangeDrawer(
        GameState game, Func<RangeDrawStyle> getDrawStyle, Func<IEnumerable<Tile>> getTilesInRange, Color color)
        : this(game, getDrawStyle, () => TileAreaBorder.From(new HashSet<Tile>(getTilesInRange())), color)
    {
    }

    public TileRangeDrawer(
        GameState game, Func<RangeDrawStyle> getDrawStyle, Func<TileAreaBorder> getTilesInRange, Color color)
    {
        this.game = game;
        this.getDrawStyle = getDrawStyle;
        this.getTilesInRange = getTilesInRange;
        this.color = color;
    }

    public void Draw()
    {
        var drawStyle = getDrawStyle();
        if (drawStyle == RangeDrawStyle.DoNotDraw)
        {
            return;
        }

        var border = getTilesInRange();
        if (border.IsEmpty)
        {
            return;
        }

        TileAreaBorderRenderer.Render(border, game, color * (drawStyle == RangeDrawStyle.DrawFull ? 0.5f : 0.25f));
    }

    public enum RangeDrawStyle
    {
        DoNotDraw,
        DrawMinimally,
        DrawFull
    }
}
