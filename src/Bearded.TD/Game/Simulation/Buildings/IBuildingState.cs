using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Shared.Proxies;

namespace Bearded.TD.Game.Simulation.Buildings;

[AutomaticProxy]
interface IBuildingState
{
    public TileRangeDrawer.RangeDrawStyle RangeDrawing { get; }
    public bool IsGhost { get; }
    public bool IsMaterialized { get; }
    public bool IsFunctional { get; }
    public bool CanApplyUpgrades { get; }
    // Whether the player can impact the current health of this building using repairs or deleting the building.
    public bool AcceptsPlayerHealthChanges { get; }
}
