using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IPermanentUpgrade : IBlueprint, IUpgrade
{
    string Name { get; }
    ResourceAmount Cost { get; }
}
