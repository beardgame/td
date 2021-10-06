using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins
{
    readonly struct ObjectRepaired : IComponentEvent
    {
        public Faction RepairedBy { get; }

        public ObjectRepaired(Faction repairedBy)
        {
            RepairedBy = repairedBy;
        }
    }
}
