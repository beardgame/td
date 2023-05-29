using System.Collections.Generic;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Drawing;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingState : IBuildingState
{
    // Derived properties (IBuildingState implementation)
    public TileRangeDrawer.RangeDrawStyle RangeDrawing => TileRangeDrawStyle.FromSelectionState(SelectionState);
    public bool IsGhost => false;
    public bool IsMaterialized { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsFunctional => IsCompleted && ActiveBreakages.Count == 0 && !IsDead;

    // Mutable state
    public SelectionState SelectionState { get; set; }
    public List<IBreakageReceipt> ActiveBreakages { get; } = new();
    public bool IsDead { get; set; }
    public bool AcceptsPlayerHealthChanges { get; set; } = true;

    public IBuildingState CreateProxy() => new BuildingStateProxy(this);
}
