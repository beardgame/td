using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IBuilding : IComponentOwner
    {
        IBuildingState State { get; }
    }
}