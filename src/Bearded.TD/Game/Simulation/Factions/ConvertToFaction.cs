using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Factions
{
    readonly struct ConvertToFaction : IComponentEvent
    {
        public Faction Faction { get; }

        public ConvertToFaction(Faction faction)
        {
            Faction = faction;
        }
    }
}
