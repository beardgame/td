using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Synchronization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncBuildings
    {
        public static ISerializableCommand<GameInstance> Command(IEnumerable<Building> buildings)
            => new Implementation(buildings
                .Select(b => b.GetComponents<ISyncer<Building>>().Single())
                .Select(syncer => (syncer, syncer.GetCurrentStateToSync()))
                .ToImmutableArray());

        private sealed class Implementation : SyncEntities.Implementation<Building>
        {
            public Implementation(IList<(ISyncer<Building>, IStateToSync)> syncers) : base(syncers) { }

            protected override ICommandSerializer<GameInstance> ToSerializer(
                IEnumerable<(ISyncer<Building>, IStateToSync)> syncedObjects) => new Serializer(syncedObjects);
        }

        private sealed class Serializer : SyncEntities.Serializer<Building>
        {
            [UsedImplicitly]
            public Serializer() { }

            public Serializer(IEnumerable<(ISyncer<Building>, IStateToSync)> syncers) : base(syncers) { }

            protected override ISerializableCommand<GameInstance> ToImplementation(
                ImmutableArray<(ISyncer<Building>, IStateToSync)> syncedObjects) => new Implementation(syncedObjects);
        }
    }
}
