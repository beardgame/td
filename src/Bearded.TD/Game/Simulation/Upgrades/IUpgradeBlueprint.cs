using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    interface IUpgradeBlueprint : IBlueprint
    {
        string Name { get; }
        ResourceAmount Cost { get; }
        IEnumerable<IUpgradeEffect> Effects { get; }
    }
}
