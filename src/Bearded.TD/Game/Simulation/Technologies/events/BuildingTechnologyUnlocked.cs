using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct BuildingTechnologyUnlocked : IGlobalEvent
    {
        public Faction Faction { get; }
        public IBuildingBlueprint Blueprint { get; }

        public BuildingTechnologyUnlocked(Faction faction, IBuildingBlueprint blueprint)
        {
            Faction = faction;
            Blueprint = blueprint;
        }
    }
}
