using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Elements;

interface IDischargeModeReport : IReport
{
    public GameObject Object { get; }
    public CapacitorDischargeMode DischargeMode { get; }
}
