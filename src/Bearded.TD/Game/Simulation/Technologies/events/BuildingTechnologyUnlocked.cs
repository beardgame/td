using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct BuildingTechnologyUnlocked : IGlobalEvent
    {
        public FactionTechnology FactionTechnology { get; }
        public IBuildingBlueprint Blueprint { get; }

        public BuildingTechnologyUnlocked(FactionTechnology factionTechnology, IBuildingBlueprint blueprint)
        {
            FactionTechnology = factionTechnology;
            Blueprint = blueprint;
        }
    }
}
