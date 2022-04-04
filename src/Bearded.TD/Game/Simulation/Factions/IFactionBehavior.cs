using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Factions;

interface IFactionBehavior
{
    void OnAdded(Faction owner, GlobalGameEvents events);
}
