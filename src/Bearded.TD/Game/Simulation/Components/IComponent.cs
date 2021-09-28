using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components
{
    interface IComponent
    {
        void Update(TimeSpan elapsedTime);
        void Draw(CoreDrawers drawers);

        bool CanApplyUpgradeEffect(IUpgradeEffect effect);
        void ApplyUpgradeEffect(IUpgradeEffect effect);
        bool RemoveUpgradeEffect(IUpgradeEffect effect);
    }

    interface IComponent<in TOwner> : IComponent
    {
        void OnAdded(TOwner owner, ComponentEvents events);
        void OnRemoved();
    }
}
