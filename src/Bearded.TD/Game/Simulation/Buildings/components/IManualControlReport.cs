using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualControlReport : IReport
{
    bool CanBeControlledBy(Faction faction);
    Position2 SubjectPosition { get; }
    Unit SubjectRange { get; }
    void StartControl(IManualTarget2 target, Action cancelControl);
    void EndControl();
}
