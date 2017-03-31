using Bearded.TD.Rendering;
using Bearded.TD.Screens;

namespace Bearded.TD.Game.UI
{
    class GameUI : ScreenLayerGroup
    {
        public GameUI(ScreenLayerCollection parent, GeometryManager geometries, GameInstance gameInstance) : base(parent)
        {
            var gameRunner = new GameRunner(gameInstance);

            AddScreenLayerOnTop(new GameWorldScreenLayer(this, gameInstance, gameRunner, geometries));
            AddScreenLayerOnTop(new BuildingScreenLayer(this, gameInstance, geometries));
            AddScreenLayerOnTop(new GameOverScreenLayer(this, geometries, gameInstance));
        }
    }
}