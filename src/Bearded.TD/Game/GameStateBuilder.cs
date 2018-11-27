using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameStateBuilder
    {
        private readonly GameInstance game;
        private readonly int radius;
        private readonly ITilemapGenerator tilemapGenerator;

        public GameStateBuilder(GameInstance game, int radius, ITilemapGenerator tilemapGenerator)
        {
            this.game = game;
            this.radius = radius;
            this.tilemapGenerator = tilemapGenerator;
        }

        public IEnumerable<ISerializableCommand<GameInstance>> Generate()
        {
            yield return CreateGameState.Command(game, radius);
            yield return AddFaction.Command(game, new Faction(game.Ids.GetNext<Faction>(), null, true, true));

            foreach (var command in setupFactions())
                yield return command;

            var baseBlueprint = game.Blueprints.Buildings[Constants.Mods.BaseBuildingId];
            var footprint = baseBlueprint.FootprintGroup.Positioned(0, game.State.Level, new Position2(0, 0));
            yield return PlopBuilding.Command(game, game.State.RootFaction, game.Meta.Ids.GetNext<Building>(), baseBlueprint, footprint);

            var tilemapTypes = tilemapGenerator.Generate(radius, UserSettings.Instance.Misc.MapGenSeed ?? StaticRandom.Int());

            var tilemapDrawInfos = drawInfosFromTypes(tilemapTypes);

            yield return FillTilemap.Command(game, tilemapTypes, tilemapDrawInfos);
            yield return BlockTilesForBuilding.Command(
                    game,
                    game.State.Level.Tilemap.SpiralCenteredAt(Tile.Origin, 3).ToList());
        }

        private IEnumerable<ISerializableCommand<GameInstance>> setupFactions()
        {
            foreach (var (p, i) in game.Players.Indexed())
            {
                var factionColor = Color.FromHSVA(i * Mathf.TwoPi / 6, 1, 1f);
                var playerFaction = new Faction(game.Ids.GetNext<Faction>(), game.State.RootFaction, false, true, p.Name, factionColor);
                yield return AddFaction.Command(game, playerFaction);
                yield return SetPlayerFaction.Command(p, playerFaction);
            }
        }

        private Tilemap<TileDrawInfo> drawInfosFromTypes(Tilemap<TileGeometry.TileType> types)
        {
            var drawInfos = new Tilemap<TileDrawInfo>(types.Radius);

            foreach (var type in types)
            {
                drawInfos[type.X, type.Y] = TileDrawInfo.For(types[type]);
            }

            return drawInfos;
        }
    }
}
