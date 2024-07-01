namespace Bearded.TD.Game.Simulation.Buildings;

sealed class GhostBuildingState : IBuildingState
{
    public RangeDrawStyle RangeDrawing => RangeDrawStyle.DrawFull;
    public bool IsGhost => true;
    public bool IsMaterialized => false;
    public bool IsCompleted => false;
    public bool IsFunctional => false;
    public bool CanApplyUpgrades => false;
    public bool AcceptsPlayerHealthChanges => false;
}
