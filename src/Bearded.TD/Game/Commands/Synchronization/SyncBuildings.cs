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
            => new Implementation(buildings.Select(u => (u, u.GetCurrentStateToSync())).ToList());

        private sealed class Implementation : SyncEntities.Implementation<Building>
        {
            public Implementation(IList<(Building, IStateToSync)> syncedObjects) : base(syncedObjects) { }

            protected override ICommandSerializer<GameInstance> ToSerializer(
                IEnumerable<(Building, IStateToSync)> syncedObjects) => new Serializer(syncedObjects);
        }

        private sealed class Serializer : SyncEntities.Serializer<Building>
        {
            [UsedImplicitly]
            public Serializer() { }

            public Serializer(IEnumerable<(Building, IStateToSync)> syncedObjects) : base(syncedObjects) { }

            protected override ISerializableCommand<GameInstance> ToImplementation(
                ImmutableArray<(Building, IStateToSync)> syncedObjects) => new Implementation(syncedObjects);
        }
    }
}
