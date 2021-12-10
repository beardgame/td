using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Zones;

namespace Bearded.TD.Game.Simulation.Exploration;

interface IZoneRevealReport : IReport
{
    Zone Zone { get; }
    bool CanRevealNow { get; }
}