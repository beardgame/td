using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualOverdriveReport : IReport
{
    bool CanBeControlledBy(Faction faction);
    void StartControl(Action cancelOverdrive);
    void EndControl();
}
