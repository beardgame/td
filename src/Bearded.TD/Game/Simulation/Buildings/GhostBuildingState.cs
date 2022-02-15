using Bearded.TD.Game.Simulation.Drawing;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class GhostBuildingState : IBuildingState
{
    public TileRangeDrawer.RangeDrawStyle RangeDrawing => TileRangeDrawer.RangeDrawStyle.DrawFull;
    public bool IsMaterialized => false;
    public bool IsFunctional => false;
    public bool CanApplyUpgrades => false;
    public bool AcceptsPlayerHealthChanges => false;
}
