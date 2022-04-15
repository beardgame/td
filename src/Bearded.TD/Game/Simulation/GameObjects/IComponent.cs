using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IComponent
{
    void OnAdded(GameObject owner, ComponentEvents events);
    void OnRemoved();
    void Update(TimeSpan elapsedTime);

    bool CanApplyUpgradeEffect(IUpgradeEffect effect);
    void ApplyUpgradeEffect(IUpgradeEffect effect);
    bool RemoveUpgradeEffect(IUpgradeEffect effect);
}
