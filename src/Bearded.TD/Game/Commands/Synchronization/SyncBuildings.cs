using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Synchronization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization;

static class SyncBuildings
{
    public static ISerializableCommand<GameInstance> Command(IEnumerable<ComponentGameObject> buildings)
        => new Implementation(buildings
            .Select(b => b.GetComponents<ISyncer<ComponentGameObject>>().Single())
            .Select(syncer => (syncer, syncer.GetCurrentStateToSync()))
            .ToImmutableArray());

    private sealed class Implementation : SyncEntities.Implementation<ComponentGameObject>
    {
        public Implementation(IList<(ISyncer<ComponentGameObject>, IStateToSync)> syncers) : base(syncers) { }

        protected override ICommandSerializer<GameInstance> ToSerializer(
            IEnumerable<(ISyncer<ComponentGameObject>, IStateToSync)> syncedObjects) =>
            new Serializer(syncedObjects);
    }

    private sealed class Serializer : SyncEntities.Serializer<ComponentGameObject>
    {
        [UsedImplicitly]
        public Serializer() { }

        public Serializer(IEnumerable<(ISyncer<ComponentGameObject>, IStateToSync)> syncers) : base(syncers) { }

        protected override ISerializableCommand<GameInstance> ToImplementation(
            ImmutableArray<(ISyncer<ComponentGameObject>, IStateToSync)> syncedObjects) =>
            new Implementation(syncedObjects);
    }
}