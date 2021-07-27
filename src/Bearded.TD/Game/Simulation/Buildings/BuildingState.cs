using System;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingState : IBuildingState
    {
        // Derived properties (IBuildingState implementation)
        public TileRangeDrawer.RangeDrawStyle RangeDrawing => toRangeDrawType(SelectionState);
        public bool IsMaterialized { get; set; }
        public bool IsFunctional => IsCompleted;
        public bool CanApplyUpgrades => IsCompleted;

        // Mutable state
        public SelectionState SelectionState { get; set; }
        public bool IsCompleted { get; set; }

        public IBuildingState CreateProxy() => new BuildingStateProxy(this);

        private static TileRangeDrawer.RangeDrawStyle toRangeDrawType(SelectionState selectionState)
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
}
