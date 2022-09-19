using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Technologies;

readonly struct BuildingTechnologyUnlocked : IGlobalEvent
{
    public FactionTechnology FactionTechnology { get; }
    public IGameObjectBlueprint Blueprint { get; }

    public BuildingTechnologyUnlocked(FactionTechnology factionTechnology, IGameObjectBlueprint blueprint)
    {
        FactionTechnology = factionTechnology;
        Blueprint = blueprint;
    }
}
