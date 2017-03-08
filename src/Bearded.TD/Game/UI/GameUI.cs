using Bearded.TD.Commands;
using Bearded.TD.Game.Generation;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.Utilities;

namespace Bearded.TD.Game.UI
{
    class GameUI : ScreenLayerGroup
    {
        public GameUI(Logger logger, GeometryManager geometries)
        {
            // these are different for clients
            var commandDispatcher = new ServerCommandDispatcher(new DefaultCommandExecutor());
            var requestDispatcher = new ServerRequestDispatcher(commandDispatcher);
            var dispatcher = new ServerDispatcher(commandDispatcher);

            var meta = new GameMeta(logger, dispatcher);

            var gameState = GameStateBuilder.Generate(meta, new DefaultTilemapGenerator(logger));
            var gameInstance = new GameInstance(
                gameState,
                new GameCamera(meta, gameState.Level.Tilemap.Radius),
                requestDispatcher
                );
            var gameRunner = new GameRunner(gameInstance);

            AddScreenLayer(new GameWorldScreenLayer(gameInstance, gameRunner, geometries));
            AddScreenLayer(new BuildingScreenLayer(gameInstance, geometries));
            AddScreenLayer(new GameOverScreenLayer(gameInstance, geometries));
        }
    }
}