using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Technologies;

readonly struct BuildingTechnologyUnlocked : IGlobalEvent
{
    public FactionTechnology FactionTechnology { get; }
    public IComponentOwnerBlueprint Blueprint { get; }

    public BuildingTechnologyUnlocked(FactionTechnology factionTechnology, IComponentOwnerBlueprint blueprint)
    {
        FactionTechnology = factionTechnology;
        Blueprint = blueprint;
    }
}