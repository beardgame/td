using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualOverdriveReport : IReport
{
    GameObject Building { get; }
    bool CanBeEnabledBy(Faction faction);
}
