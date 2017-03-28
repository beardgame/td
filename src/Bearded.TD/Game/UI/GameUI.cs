using Bearded.TD.Commands;
using Bearded.TD.Game.Generation;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.Utilities;

namespace Bearded.TD.Game.UI
{
    class GameUI : ScreenLayerGroup
    {
        public GameUI(ScreenLayerCollection parent, GeometryManager geometries, Logger logger) : base(parent)
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

            AddScreenLayerOnTop(new GameWorldScreenLayer(this, gameInstance, gameRunner, geometries));
            AddScreenLayerOnTop(new BuildingScreenLayer(this, gameInstance, geometries));
            AddScreenLayerOnTop(new GameOverScreenLayer(this, geometries, gameInstance));
        }
    }
}