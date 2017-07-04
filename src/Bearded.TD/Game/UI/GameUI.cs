using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Game.UI
{
    class GameUI : ScreenLayerGroup
    {
        public GameUI(ScreenLayerCollection parent, GeometryManager geometries, GameInstance gameInstance, NetworkInterface networkInterface, InputManager inputManager) : base(parent)
        {
            var gameRunner = new GameRunner(gameInstance, networkInterface, inputManager);

            AddScreenLayerOnTop(new GameWorldScreenLayer(this, gameInstance, gameRunner, geometries));
            AddScreenLayerOnTop(new ActionBarScreenLayer(this, geometries, gameInstance, inputManager));
            AddScreenLayerOnTop(new StatusScreenLayer(this, geometries, gameInstance));
            AddScreenLayerOnTop(new GameOverScreenLayer(this, geometries, gameInstance));
        }
    }
}