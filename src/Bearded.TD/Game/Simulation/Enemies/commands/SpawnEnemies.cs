using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Enemies;

static class SpawnEnemies
{
    private static readonly Unit spawnOffset = 0.01.U();

    public static ISerializableCommand<GameInstance> Command(
        GameState state,
        EnemyForm form,
        IReadOnlyList<Id<GameObject>> ids,
        Position3 position) => new Implementation(state, form, ids, position);

    private sealed class Implementation(
        GameState state,
        EnemyForm form,
        IReadOnlyList<Id<GameObject>> ids,
        Position3 position) : ISerializableCommand<GameInstance>
    {
        public void Execute()
        {
            // We slightly offset the spawns from the position. This makes it more likely that collision pushes the
            // enemies in similar directions between client and server rather than randomly moving in different
            // directions and having to rely on a later synchronization cycle to match up enemy locations.
            var spawnOffset = ids.Count > 1 ? SpawnEnemies.spawnOffset : Unit.Zero;
            var angleBetweenSpawns = Angle.FromDegrees(360f / ids.Count);

            for (var i = 0; i < ids.Count; i++)
            {
                var direction = Direction2.Zero + i * angleBetweenSpawns;
                var pos = position + direction.Step(spawnOffset).WithZ();
                var enemy = EnemyFactory.Create(ids[i], form, pos);
                state.Add(enemy);
            }
        }

        public ICommandSerializer<GameInstance> Serializer => new Serializer(form, ids, position);
    }

    // ReSharper disable once EmptyConstructor
    private sealed class Serializer() : ICommandSerializer<GameInstance>
    {
        private ModAwareId enemyBlueprint = ModAwareId.Invalid;
        private (string Socket, ModAwareId Module)[] modules = [];
        private (DamageType DamageType, float Resistance)[] resistances = [];
        private Id<GameObject>[] ids = [];
        private Position3 position;

        public Serializer(EnemyForm form, IReadOnlyList<Id<GameObject>> ids, Position3 position) : this()
        {
            enemyBlueprint = form.Blueprint.Id;
            modules = form.Modules.Select(kvp => (kvp.Key.Id, kvp.Value.Id)).ToArray();
            resistances = form.Resistances.Select(kvp => (kvp.Key, kvp.Value.NumericValue)).ToArray();
            this.ids = ids.ToArray();
            this.position = position;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            var resolvedBlueprint = game.Blueprints.GameObjects[enemyBlueprint];
            var resolvedModules = modules.ToImmutableDictionary(
                tuple => SocketShape.FromLiteral(tuple.Socket),
                tuple => game.Blueprints.Modules[tuple.Module]);
            var resolvedResistances = resistances.ToImmutableDictionary(
                tuple => tuple.DamageType,
                tuple => new Resistance(tuple.Resistance));
            var form = new EnemyForm(resolvedBlueprint, resolvedModules, resolvedResistances);
            return new Implementation(game.State, form, [..ids], position);
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref enemyBlueprint);
            stream.SerializeArrayCount(ref modules);
            for (var i = 0; i < modules.Length; i++)
            {
                stream.Serialize(ref modules[i].Socket);
                stream.Serialize(ref modules[i].Module);
            }
            stream.SerializeArrayCount(ref resistances);
            for (var i = 0; i < resistances.Length; i++)
            {
                stream.Serialize(ref resistances[i].DamageType);
                stream.Serialize(ref resistances[i].Resistance);
            }
            stream.SerializeArrayCount(ref ids);
            for (var i = 0; i < ids.Length; i++)
            {
                stream.Serialize(ref ids[i]);
            }
            stream.Serialize(ref position);
        }
    }
}
