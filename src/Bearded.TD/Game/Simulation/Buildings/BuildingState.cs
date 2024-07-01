using System;
using System.Collections.Generic;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings.Ruins;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingState : IBuildingState
{
    // Derived properties (IBuildingState implementation)
    [Obsolete("Should be handled by UI code")]
    public RangeDrawStyle RangeDrawing => TileRangeDrawStyle.FromSelectionState(SelectionState);
    public bool IsGhost => false;
    public bool IsMaterialized { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsFunctional => IsCompleted && ActiveBreakages.Count == 0 && !IsDead;

    // Mutable state
    public SelectionState SelectionState { get; set; }
    public List<IBreakageReceipt> ActiveBreakages { get; } = [];
    public bool IsDead { get; set; }
    public bool AcceptsPlayerHealthChanges { get; set; } = true;

    public IBuildingState CreateProxy() => new BuildingStateProxy(this);
}
