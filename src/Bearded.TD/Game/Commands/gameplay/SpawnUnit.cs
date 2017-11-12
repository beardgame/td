﻿using Bearded.TD.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SpawnUnit
    {
        public static ICommand Command(
                GameState game, Tile<TileInfo> tile, UnitBlueprint blueprint, Id<GameUnit> unitId)
            => new Implementation(game, tile, blueprint, unitId);

        private class Implementation : ICommand
        {
            private readonly GameState game;
            private readonly Tile<TileInfo> tile;
            private readonly UnitBlueprint blueprint;
            private readonly Id<GameUnit> unitId;

            public Implementation(GameState game, Tile<TileInfo> tile, UnitBlueprint blueprint, Id<GameUnit> unitId)
            {
                this.game = game;
                this.tile = tile;
                this.blueprint = blueprint;
                this.unitId = unitId;
            }

            public void Execute() => game.Add(new EnemyUnit(unitId, blueprint, tile));

            public ICommandSerializer Serializer => new Serializer(blueprint, tile, unitId);
        }

        private class Serializer : ICommandSerializer
        {
            private string blueprint;
            private int x;
            private int y;
            private Id<GameUnit> unitId;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(UnitBlueprint blueprint, Tile<TileInfo> tile, Id<GameUnit> unitId)
            {
                this.blueprint = blueprint.Name;
                x = tile.X;
                y = tile.Y;
                this.unitId = unitId;
            }

            public ICommand GetCommand(GameInstance game) => new Implementation(
                game.State,
                new Tile<TileInfo>(game.State.Level.Tilemap, x, y),
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