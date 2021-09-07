using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Shared.Proxies;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [AutomaticProxy]
    interface IBuildingState
    {
        public TileRangeDrawer.RangeDrawStyle RangeDrawing { get; }
        public bool IsMaterialized { get; }
        public bool IsFunctional { get; }
        public bool CanApplyUpgrades { get; }
    }
}
