using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Synchronization;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Synchronization;

sealed class Syncer : Component, ISyncer
{
    private IIdProvider? idProvider;
    public Id<GameObject> GameObjectId =>
        idProvider?.Id ?? throw new InvalidOperationException("Synced object must have an ID.");

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IIdProvider>(Owner, Events, provider => idProvider = provider);
    }

    public override void Activate()
    {
        base.Activate();
        Owner.Game.Meta.Synchronizer.RegisterSyncable(Owner);
    }

    public override void Update(TimeSpan elapsedTime) {}

    public IStateToSync GetCurrentStateToSync()
    {
        return new CompositeStateToSync(
            Owner.GetComponents<ISyncable>().Select(s => s.GetCurrentStateToSync()).ToImmutableArray());
    }
}
