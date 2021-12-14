using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Factions;

interface IFactionBehavior<in TOwner>
{
    void OnAdded(TOwner owner, GlobalGameEvents events);
}