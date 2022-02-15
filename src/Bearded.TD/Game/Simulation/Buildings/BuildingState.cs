using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Drawing;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingState : IBuildingState
{
    // Derived properties (IBuildingState implementation)
    public TileRangeDrawer.RangeDrawStyle RangeDrawing => TileRangeDrawStyle.FromSelectionState(SelectionState);
    public bool IsMaterialized { get; set; }
    public bool IsFunctional => IsCompleted && !IsRuined && !IsDead;
    public bool CanApplyUpgrades => IsCompleted && !IsRuined && !IsDead;

    // Mutable state
    public SelectionState SelectionState { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRuined { get; set; }
    public bool IsDead { get; set; }
    public bool AcceptsPlayerHealthChanges { get; set; } = true;

    public IBuildingState CreateProxy() => new BuildingStateProxy(this);
}
