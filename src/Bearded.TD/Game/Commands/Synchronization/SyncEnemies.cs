using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Synchronization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization;

static class SyncEnemies
{
    public static ISerializableCommand<GameInstance> Command(IEnumerable<EnemyUnit> units)
        => new Implementation(units
            .Select(u => ((IComponentOwner) u).GetComponents<ISyncer<EnemyUnit>>().Single())
            .Select(syncer => (syncer, syncer.GetCurrentStateToSync()))
            .ToImmutableArray());

    private sealed class Implementation : SyncEntities.Implementation<EnemyUnit>
    {
        public Implementation(IList<(ISyncer<EnemyUnit>, IStateToSync)> syncers) : base(syncers) { }

        protected override ICommandSerializer<GameInstance> ToSerializer
            (IEnumerable<(ISyncer<EnemyUnit>, IStateToSync)> syncedObjects) => new Serializer(syncedObjects);
    }

    private sealed class Serializer : SyncEntities.Serializer<EnemyUnit>
    {
        [UsedImplicitly]
        public Serializer() { }

        public Serializer(IEnumerable<(ISyncer<EnemyUnit>, IStateToSync)> syncers) : base(syncers) { }

        protected override ISerializableCommand<GameInstance> ToImplementation(
            ImmutableArray<(ISyncer<EnemyUnit>, IStateToSync)> syncedObjects) => new Implementation(syncedObjects);
    }
}