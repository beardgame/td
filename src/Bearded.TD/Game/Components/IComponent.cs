using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Components.Events;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    interface IComponent
    {
        void Update(TimeSpan elapsedTime);
        void Draw(GeometryManager geometries);

        bool CanApplyUpgradeEffect(IUpgradeEffect effect);
        void ApplyUpgradeEffect(IUpgradeEffect effect);
        bool RemoveUpgradeEffect(IUpgradeEffect effect);
    }

    interface IComponent<TOwner> : IBehavior<TOwner>, IComponent
    {
        void OnAdded(TOwner owner, ComponentEvents events);
    }
}
