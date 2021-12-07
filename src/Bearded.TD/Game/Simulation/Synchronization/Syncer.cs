using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Synchronization;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Synchronization
{
    sealed class Syncer<T> : Component<T>, ISyncer<T>
        where T : IComponentOwner, IDeletable, IGameObject
    {
        private IIdProvider<T>? idProvider;
        public Id<T> EntityId =>
            idProvider?.Id ?? throw new InvalidOperationException("Synced object must have an ID.");

        protected override void OnAdded()
        {
            ComponentDependencies.Depend<IIdProvider<T>>(Owner, Events, provider => idProvider = provider);
            Owner.Game.Meta.Synchronizer.RegisterSyncable(Owner);
        }

        public override void Update(TimeSpan elapsedTime) {}

        public IStateToSync GetCurrentStateToSync()
        {
            return new CompositeStateToSync(Owner.GetComponents<ISyncable>().Select(s => s.GetCurrentStateToSync()));
        }
    }
}
