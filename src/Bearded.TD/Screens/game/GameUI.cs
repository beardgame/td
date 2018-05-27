using Bearded.TD.Game;
using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.Utilities.Input;

namespace Bearded.TD.Screens
{
    class GameUI : ScreenLayerGroup
    {
        public GameUI(ScreenLayerCollection parent, GeometryManager geometries, GameInstance gameInstance, NetworkInterface networkInterface, InputManager inputManager) : base(parent)
        {
            var gameRunner = new GameRunner(gameInstance, networkInterface);

            AddScreenLayerOnTop(new GameWorldScreenLayer(this, gameInstance, gameRunner, geometries));
            AddScreenLayerOnTop(new ActionBarScreenLayer(this, geometries, gameInstance, inputManager));
            AddScreenLayerOnTop(new StatusScreenLayer(this, geometries, gameInstance));
            AddScreenLayerOnTop(new GameOverScreenLayer(this, geometries, gameInstance));
#if DEBUG
            AddScreenLayerOnTop(new DebugScreenLayer(this, geometries, gameInstance, inputManager));
#endif
        }
    }
}