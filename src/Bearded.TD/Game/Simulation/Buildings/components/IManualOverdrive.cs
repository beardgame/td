using System;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualOverdrive
{
    bool CanBeEnabledBy(Faction faction);
    void StartOverdrive(Action cancelOverdrive);
    void EndOverdrive();
}
