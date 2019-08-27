using System.Collections.Generic;
using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Components
{
    interface IComponentOwnerBlueprint : IBlueprint
    {
        IEnumerable<IComponent<TOwner>> GetComponents<TOwner>();

        bool CanApplyUpgradeEffect<TOwner>(IUpgradeEffect effect);
    }
}
