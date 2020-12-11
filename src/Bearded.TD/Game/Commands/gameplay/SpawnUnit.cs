using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class SpawnUnit
    {
        public static ISerializableCommand<GameInstance> Command(
                GameState game, Tile tile, IUnitBlueprint blueprint, Id<EnemyUnit> unitId)
            => new Implementation(game, tile, blueprint, unitId);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameState game;
            private readonly Tile tile;
            private readonly IUnitBlueprint blueprint;
            private readonly Id<EnemyUnit> unitId;

            public Implementation(GameState game, Tile tile, IUnitBlueprint blueprint, Id<EnemyUnit> unitId)
            {
                this.game = game;
                this.tile = tile;
                this.blueprint = blueprint;
                this.unitId = unitId;
            }

            public void Execute() => game.Add(new EnemyUnit(unitId, blueprint, tile));

            public ICommandSerializer<GameInstance> Serializer => new Serializer(blueprint, tile, unitId);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private ModAwareId blueprint;
            private int x;
            private int y;
            private Id<EnemyUnit> unitId;

            [UsedImplicitly]
            public Serializer()
            {
            }

            public Serializer(IBlueprint blueprint, Tile tile, Id<EnemyUnit> unitId)
            {
                this.blueprint = blueprint.Id;
                x = tile.X;
                y = tile.Y;
                this.unitId = unitId;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => new Implementation(
                game.State,
                new Tile(x, y),
                game.Blueprints.Units[blueprint],
                unitId);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref blueprint);
                stream.Serialize(ref x);
                stream.Serialize(ref y);
                stream.Serialize(ref unitId);
            }
        }
    }
}
