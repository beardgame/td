using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IComponentOwnerBlueprint : IBlueprint
{
    IEnumerable<IComponent> GetComponents();
    bool CanApplyUpgradeEffect(IUpgradeEffect effect);
}
