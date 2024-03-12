namespace Bearded.TD.Game.Simulation.Buildings;

interface IUpgradeSlots
{
    int TotalSlotsCount { get; }
    int FilledSlotsCount { get; }
    bool HasAvailableSlot => FilledSlotsCount < TotalSlotsCount;

    void FillSlot();
}
