using Bearded.TD.Game.GameState.Components.Events;
using Bearded.TD.Game.GameState.Upgrades;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Components
{
    interface IComponent
    {
        void Update(TimeSpan elapsedTime);
        void Draw(GeometryManager geometries);

        bool CanApplyUpgradeEffect(IUpgradeEffect effect);
        void ApplyUpgradeEffect(IUpgradeEffect effect);
        bool RemoveUpgradeEffect(IUpgradeEffect effect);
    }

    interface IComponent<in TOwner> : IComponent
    {
        void OnAdded(TOwner owner, ComponentEvents events);
    }
}
