using System.Collections.Generic;
using Bearded.TD.Game.GameState.Resources;

namespace Bearded.TD.Game.GameState.Upgrades
{
    interface IUpgradeBlueprint : IBlueprint
    {
        string Name { get; }
        ResourceAmount Cost { get; }
        IEnumerable<IUpgradeEffect> Effects { get; }
    }
}
