namespace Bearded.TD.Game.Simulation.Buildings;

interface IUpgradeSlots
{
    int TotalSlotsCount { get; }
    int FilledSlotsCount { get; }
    int ReservedSlotsCount { get; }
    bool HasAvailableSlot => FilledSlotsCount + ReservedSlotsCount < TotalSlotsCount;

    IUpgradeSlotReservation ReserveSlot();
}

interface IUpgradeSlotReservation
{
    void Fill();
}
