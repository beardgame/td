using Bearded.TD.Game.Simulation.Reports;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IManualControlReport : IReport
    {
        Position2 SubjectPosition { get; }
        void StartControl(IManualTarget2 target);
        void EndControl();
    }
}
