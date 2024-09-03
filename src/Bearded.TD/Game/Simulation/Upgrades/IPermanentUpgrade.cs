using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IPermanentUpgrade : IBlueprint, IUpgrade
{
    string Name { get; }
    Resource<Scrap> Cost { get; }
    Element Element { get; }
}
