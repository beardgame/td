using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Input;
using OpenTK;

namespace Bearded.TD.Screens
{
    class GameWorldScreenLayer : ScreenLayer
    {
        private readonly GameInstance game;
        private readonly GameRunner runner;
        private readonly GeometryManager geometries;

        public override Matrix4 ViewMatrix => game.Camera.ViewMatrix;

        public override RenderOptions RenderOptions => RenderOptions.Game;

        public GameWorldScreenLayer(ScreenLayerCollection parent, GameInstance game, GameRunner runner, GeometryManager geometries) : base(parent)
        {
            this.game = game;
            this.runner = runner;
            this.geometries = geometries;
        }

        public override void HandleInput(UpdateEventArgs args, InputState inputState)
        {
            runner.HandleInput(args, inputState);
        }

        public override void Update(UpdateEventArgs args)
        {
            runner.Update(args);
        }

        public override void Draw()
        {
            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, -1);

            game.State.Level.Draw(geometries);


            var radius = game.State.Level.Tilemap.Radius;

            geometries.PointLight.Draw(new Vector3(-radius * 2, radius * 2, radius), radius * 10, Color.White * 0.15f);

            foreach (var obj in game.State.GameObjects)
            {
                obj.Draw(geometries);
            }
            
            var debugPathfinding = UserSettings.Instance.Debug.Pathfinding;
            if (debugPathfinding > 0)
            {
                game.State.Navigator.DrawDebug(geometries, game.State.Level, debugPathfinding > 1);
            }
        }

        protected override void OnViewportSizeChanged()
        {
            base.OnViewportSizeChanged();
            game.Camera.OnViewportSizeChanged(ViewportSize);
        }
    }
}
