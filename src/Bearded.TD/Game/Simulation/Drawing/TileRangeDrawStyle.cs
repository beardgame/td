using System;
using Bearded.TD.Game.Meta;

namespace Bearded.TD.Game.Simulation.Drawing;

static class TileRangeDrawStyle
{
    public static TileRangeDrawer.RangeDrawStyle FromSelectionState(SelectionState selectionState)
    {
        return selectionState switch
        {
            SelectionState.Default => TileRangeDrawer.RangeDrawStyle.DoNotDraw,
            SelectionState.Focused => TileRangeDrawer.RangeDrawStyle.DrawMinimally,
            SelectionState.Selected => TileRangeDrawer.RangeDrawStyle.DrawFull,
            _ => throw new ArgumentOutOfRangeException(nameof(selectionState), selectionState, null)
        };
    }
}
