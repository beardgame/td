using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Game.UI
{
    class GameUI : ScreenLayerGroup
    {
        public GameUI(ScreenLayerCollection parent, GeometryManager geometries, GameInstance gameInstance, InputManager inputManager) : base(parent)
        {
            var gameRunner = new GameRunner(gameInstance, inputManager);

            AddScreenLayerOnTop(new GameWorldScreenLayer(this, gameInstance, gameRunner, geometries));
            AddScreenLayerOnTop(new BuildingScreenLayer(this, gameInstance, geometries));
            AddScreenLayerOnTop(new GameOverScreenLayer(this, geometries, gameInstance));
        }
    }
}