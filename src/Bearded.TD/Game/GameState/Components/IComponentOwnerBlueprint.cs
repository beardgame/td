using System.Collections.Generic;
using Bearded.TD.Game.GameState.Upgrades;

namespace Bearded.TD.Game.GameState.Components
{
    interface IComponentOwnerBlueprint : IBlueprint
    {
        IEnumerable<IComponent<TOwner>> GetComponents<TOwner>();

        bool CanApplyUpgradeEffect<TOwner>(IUpgradeEffect effect);
    }
}
