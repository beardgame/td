using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Components
{
    interface IComponentOwnerBlueprint : IBlueprint
    {
        IEnumerable<IComponent<TOwner>> GetComponents<TOwner>();

        bool CanApplyUpgradeEffect<TOwner>(IUpgradeEffect effect);
    }
}
