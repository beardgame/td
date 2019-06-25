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
    
    interface IComponent<TOwner> : IComponent
    {
        TOwner Owner { get; }

        void OnAdded(TOwner owner);
    }
}
