using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Synchronization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncEnemies
    {
        public static ISerializableCommand<GameInstance> Command(IEnumerable<EnemyUnit> units)
            => new Implementation(units.Select(u => (u, u.GetCurrentStateToSync())).ToList());

        private sealed class Implementation : SyncEntities.Implementation<EnemyUnit>
        {
            public Implementation(IList<(EnemyUnit, IStateToSync)> syncedObjects) : base(syncedObjects) { }

            protected override ICommandSerializer<GameInstance> ToSerializer
                (IEnumerable<(EnemyUnit, IStateToSync)> syncedObjects) => new Serializer(syncedObjects);
        }

        private sealed class Serializer : SyncEntities.Serializer<EnemyUnit>
        {
            [UsedImplicitly]
            public Serializer() { }

            public Serializer(IEnumerable<(EnemyUnit, IStateToSync)> syncedObjects) : base(syncedObjects) { }

            protected override ISerializableCommand<GameInstance> ToImplementation(
                ImmutableArray<(EnemyUnit, IStateToSync)> syncedObjects) => new Implementation(syncedObjects);
        }
    }
}
