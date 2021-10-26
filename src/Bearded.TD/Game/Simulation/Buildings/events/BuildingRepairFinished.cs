using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    readonly struct BuildingRepairFinished : IGlobalEvent
    {
        public string Name { get; }
        public IGameObject GameObject { get; }

        public BuildingRepairFinished(string name, IGameObject gameObject)
        {
            Name = name;
            GameObject = gameObject;
        }
    }
}
