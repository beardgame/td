using System.Collections.Generic;

namespace Bearded.TD.Game.Upgrades
{
    interface IUpgradeBlueprint : IBlueprint
    {
        string Name { get; }
        double Cost { get; }
        IEnumerable<IUpgradeEffect> Effects { get; }
    }
}
