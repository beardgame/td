using System;
using Bearded.TD.Game.Meta;

namespace Bearded.TD.Game.Simulation.Buildings;

static class TileRangeDrawStyle
{
    public static RangeDrawStyle FromSelectionState(SelectionState selectionState)
    {
        return selectionState switch
        {
            SelectionState.Default => RangeDrawStyle.DoNotDraw,
            SelectionState.Focused => RangeDrawStyle.DrawMinimally,
            SelectionState.Selected => RangeDrawStyle.DrawFull,
            _ => throw new ArgumentOutOfRangeException(nameof(selectionState), selectionState, null)
        };
    }
}
