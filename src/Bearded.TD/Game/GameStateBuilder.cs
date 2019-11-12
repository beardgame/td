using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    sealed class GameStateBuilder
    {
        private readonly GameInstance game;
        private readonly GameSettings gameSettings;
        private readonly ITilemapGenerator tilemapGenerator;

        public GameStateBuilder(GameInstance game, ITilemapGenerator tilemapGenerator)
        {
            this.game = game;
            gameSettings = game.GameSettings;
            this.tilemapGenerator = tilemapGenerator;
        }

        public IEnumerable<ISerializableCommand<GameInstance>> Generate()
        {
            yield return CreateGameState.Command(game, gameSettings);
            yield return AddFaction.Command(game, new Faction(
                game.Ids.GetNext<Faction>(),
                game.State,
                name: "All players",
                parent: null,
                hasResources: true,
                hasWorkerNetwork: true,
                hasWorkers: true));
            yield return UnlockInitialTechnologies.Command(game, game.State.RootFaction);

            foreach (var command in setupFactions())
                yield return command;

            var baseBlueprint = game.Blueprints.Buildings[Constants.Mods.BaseBuildingId];
            var footprint = baseBlueprint.FootprintGroup.Positioned(0, game.State.Level, new Position2(0, 0));
            yield return PlopBuilding.Command(game, game.State.RootFaction, game.Meta.Ids.GetNext<Building>(), baseBlueprint, footprint);

            var tilemapTypes = tilemapGenerator.Generate(gameSettings.LevelSize, gameSettings.Seed);

            var tilemapDrawInfos = drawInfosFromTypes(tilemapTypes);

            yield return FillTilemap.Command(game, tilemapTypes, tilemapDrawInfos);
            yield return BlockTilesForBuilding.Command(
                    game,
                    Tilemap.GetSpiralCenteredAt(Tile.Origin, 3).ToList());
            yield return TurnCrevicesIntoFluidSinks.Command(game);
            yield return TurnEdgesIntoFluidSinks.Command(game);
        }

        private IEnumerable<ISerializableCommand<GameInstance>> setupFactions()
        {
            foreach (var (p, i) in game.Players.Indexed())
            {
                var factionColor = Color.FromHSVA(i * Mathf.TwoPi / 6, 1, 1f);
                var playerFaction = new Faction(
                    game.Ids.GetNext<Faction>(),
                    game.State,
                    parent: game.State.RootFaction,
                    hasResources: false,
                    hasWorkerNetwork: false,
                    hasWorkers: gameSettings.WorkerDistributionMethod != WorkerDistributionMethod.Neutral,
                    name: p.Name,
                    color: factionColor);
                yield return AddFaction.Command(game, playerFaction);
                yield return SetPlayerFaction.Command(p, playerFaction);
            }
        }

        private Tilemap<TileDrawInfo> drawInfosFromTypes(Tilemap<TileGeometry> tileGeometries)
        {
            var drawInfos = new Tilemap<TileDrawInfo>(tileGeometries.Radius);

            foreach (var tile in tileGeometries)
            {
                drawInfos[tile] = TileDrawInfo.For(tileGeometries[tile]);
            }

            return drawInfos;
        }
    }
}
