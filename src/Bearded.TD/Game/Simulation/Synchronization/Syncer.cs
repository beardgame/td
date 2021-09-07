using System.Linq;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    sealed class Syncer<T> : Component<T>, ISyncer<T>
        where T : IComponentOwner, IDeletable, IGameObject, IIdable<T>
    {
        public Id<T> EntityId => Owner.Id;

        protected override void Initialize()
        {
            Owner.Game.Meta.Synchronizer.RegisterSyncable(Owner);
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}

        public IStateToSync GetCurrentStateToSync()
        {
            return new CompositeStateToSync(Owner.GetComponents<ISyncable>().Select(s => s.GetCurrentStateToSync()));
        }
    }
}
