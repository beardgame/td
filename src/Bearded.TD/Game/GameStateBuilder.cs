using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
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
            yield return AddFaction.Command(game, new Faction(game.Ids.GetNext<Faction>(), null, true));

            var baseBlueprint = game.Blueprints.Buildings[Constants.Mods.BaseBuildingId];
            var footprint = baseBlueprint.FootprintGroup.Positioned(0, game.State.Level, new Position2(0, 0));
            yield return PlopBuilding.Command(game, game.State.RootFaction, game.Meta.Ids.GetNext<Building>(), baseBlueprint, footprint);

            var tilemapTypes = tilemapGenerator.Generate(radius, UserSettings.Instance.Misc.MapGenSeed ?? StaticRandom.Int());

            var tilemapDrawInfos = drawInfosFromTypes(tilemapTypes);

            yield return FillTilemap.Command(game, tilemapTypes, tilemapDrawInfos);
            yield return BlockTilesForBuilding.Command(
                    game,
                    game.State.Level.Tilemap.SpiralCenteredAt(
                            new Tile<TileInfo>(game.State.Level.Tilemap, 0, 0), 3).ToList());
        }

        private Tilemap<TileDrawInfo> drawInfosFromTypes(Tilemap<TileInfo.Type> types)
        {
            var drawInfos = new Tilemap<TileDrawInfo>(types.Radius);

            foreach (var type in types)
            {
                drawInfos[type.X, type.Y] = TileDrawInfo.For(type.Info);
            }

            return drawInfos;
        }
    }
}
