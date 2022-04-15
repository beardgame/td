namespace Bearded.TD.Game.Simulation.Reports;

enum ReportType : byte
{
    // The order of report types below is the same order in which reports will be shown in the report screen.

    EntityProperties,
    EntityProgression,
    EntityActions,
    Effectivity,
    ManualControl,
    Upgrades,
    Debug,
}
