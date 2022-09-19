using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

readonly struct BuildingConstructionStarted : IGlobalEvent
{
    public GameObject Building { get; }

    public BuildingConstructionStarted(GameObject building)
    {
        Building = building;
    }
}
