using Bearded.TD.Shared.Proxies;

namespace Bearded.TD.Game.Simulation.Buildings;

[AutomaticProxy]
interface IBuildingState
{
    public RangeDrawStyle RangeDrawing { get; }
    public bool IsGhost { get; }
    public bool IsMaterialized { get; }
    public bool IsCompleted { get; }
    public bool IsFunctional { get; }
    // Whether the player can impact the current health of this building using repairs or deleting the building.
    public bool AcceptsPlayerHealthChanges { get; }
}
