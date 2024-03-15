using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IUpgradeSlots
{
    IReadOnlyList<IUpgradeSlot> Slots { get; }
    int TotalSlotsCount => Slots.Count;
    int FilledSlotsCount => Slots.Count(s => s.Filled);
    bool HasAvailableSlot => FilledSlotsCount < TotalSlotsCount;

    void FillSlot(IPermanentUpgrade upgrade);
}
