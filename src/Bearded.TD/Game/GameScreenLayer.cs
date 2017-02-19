using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.Game
{
    class GameScreenLayer : ScreenLayer
    {
        private readonly GameInstance game;
        private readonly GameRunner runner;
        private readonly GeometryManager geometries;

        public override Matrix4 ViewMatrix => game.Camera.ViewMatrix;

        public GameScreenLayer(GameInstance game, GameRunner runner, GeometryManager geometries)
        {
            this.game = game;
            this.runner = runner;
            this.geometries = geometries;
        }

        public override bool HandleInput(UpdateEventArgs args)
        {
            runner.HandleInput(args);
            return false;
        }

        public override void Update(UpdateEventArgs args)
        {
            runner.Update(args);
        }

        public override void Draw()
        {
            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, -1);

            game.State.Level.Draw(geometries);
            game.State.Navigator.DrawDebug(geometries, game.State.Level);

            foreach (var obj in game.State.GameObjects)
            {
                obj.Draw(geometries);
            }
        }

        protected override void OnViewportSizeChanged()
        {
            base.OnViewportSizeChanged();
            game.Camera.OnViewportSizeChanged(ViewportSize);
        }
    }
}
