using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IManualControlReport : IReport
    {
        void StartControl(IManualTarget2 target);
        void EndControl();
    }
}
