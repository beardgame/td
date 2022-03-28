using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components;

interface IComponent
{
    void OnAdded(ComponentGameObject owner, ComponentEvents events);
    void OnRemoved();
    void Update(TimeSpan elapsedTime);

    bool CanApplyUpgradeEffect(IUpgradeEffect effect);
    void ApplyUpgradeEffect(IUpgradeEffect effect);
    bool RemoveUpgradeEffect(IUpgradeEffect effect);
}
