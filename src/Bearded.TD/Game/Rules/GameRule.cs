using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Events;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Rules
{
    abstract class GameRule : IComponent<GameState>
    {
        public void OnAdded(GameState owner, ComponentEvents _) => OnAdded(owner, owner.Meta.Events);

        protected void OnAdded(GameState owner, GlobalGameEvents events)
        {

        }

        public virtual void Update(TimeSpan elapsedTime) { }

        public virtual void Draw(GeometryManager geometries) { }

        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => false;

        public void ApplyUpgradeEffect(IUpgradeEffect effect) { }

        public bool RemoveUpgradeEffect(IUpgradeEffect effect) => false;
    }
}
