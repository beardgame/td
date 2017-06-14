using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
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
            yield return PlopBuilding.Command(game, game.State.RootFaction, baseBlueprint, footprint);

            var tilemapTypes = tilemapGenerator.Generate(radius);

            yield return FillTilemap.Command(game, tilemapTypes);

            // TODO: fill in remaining stuff from getGameStateFromTilemap
        }

        private static GameState getGameStateFromTilemap(GameMeta meta, Tilemap<TileInfo> tilemap)
        {
            var gameState = new GameState(meta, new Level(tilemap));
            //gameState.Add(new Base(Footprint.CircleSeven.Positioned(gameState.Level, new Position2()), gameState.RootFaction));
            var center = new Tile<TileInfo>(tilemap, 0, 0);
            Directions.All.Enumerate()
                .Select((d) => center.Offset(d.Step() * tilemap.Radius))
                .ForEach((t) => gameState.Add(new UnitSource(t)));

            return gameState;
        }
    }
}
