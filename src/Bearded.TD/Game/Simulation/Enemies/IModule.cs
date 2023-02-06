using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Enemies;

interface IModule : IBlueprint, IUpgrade
{
    Element AffinityElement { get; }
    SocketShape SocketShape { get; }
}
