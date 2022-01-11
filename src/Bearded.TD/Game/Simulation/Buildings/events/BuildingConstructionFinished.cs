using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings;

readonly struct BuildingConstructionFinished : IGlobalEvent
{
    public string Name { get; }
    public IGameObject GameObject { get; }

    public BuildingConstructionFinished(string name, IGameObject gameObject)
    {
        Name = name;
        GameObject = gameObject;
    }
}
