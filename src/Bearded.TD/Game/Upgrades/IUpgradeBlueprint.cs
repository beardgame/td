using System.Collections.Generic;
using Bearded.TD.Game.Resources;

namespace Bearded.TD.Game.Upgrades
{
    interface IUpgradeBlueprint : IBlueprint
    {
        string Name { get; }
        ResourceAmount Cost { get; }
        IEnumerable<IUpgradeEffect> Effects { get; }
    }
}
