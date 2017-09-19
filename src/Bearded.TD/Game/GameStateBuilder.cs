using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
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

        public IEnumerable<ICommand> Generate()
        {
            yield return CreateGameState.Command(game, radius);
            yield return AddFaction.Command(game, new Faction(game.Ids.GetNext<Faction>(), null, true));

            var baseBlueprint = game.Blueprints.Buildings["base"];
            var footprint = baseBlueprint.Footprints.Footprints[0].Positioned(game.State.Level, new Position2(0, 0));
            yield return PlopBuilding.Command(game, game.State.RootFaction, game.Meta.Ids.GetNext<Building>(), baseBlueprint, footprint);

            var tilemapTypes = tilemapGenerator.Generate(radius);

            var tilemapDrawInfos = drawInfosFromTypes(tilemapTypes);

            yield return FillTilemap.Command(game, tilemapTypes, tilemapDrawInfos);
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
