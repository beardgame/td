using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    interface IUpgradeReport : IReport
    {
        IUpgradeReportInstance CreateInstance(GameInstance game);
    }
}
