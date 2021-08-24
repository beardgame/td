using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IBuilding : IComponentOwner, IDamageTarget
    {
        IBuildingState State { get; }
    }
}
