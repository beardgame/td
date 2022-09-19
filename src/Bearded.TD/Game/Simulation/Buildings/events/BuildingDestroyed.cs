using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

readonly struct BuildingDestroyed : IGlobalEvent
{
    public GameObject Building { get; }

    public BuildingDestroyed(GameObject building)
    {
        Building = building;
    }
}
