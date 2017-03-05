using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI;
using OpenTK;

namespace Bearded.TD.Game.UI
{
    class GameOverScreenLayer : UIScreenLayer
    {
        private readonly GameInstance game;

        public GameOverScreenLayer(GameInstance game, GeometryManager geometries)
            : base(geometries, .5f, .5f, true)
        {
            this.game = game;
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            // Disable inputs below if game over.
            return !game.State.Meta.GameOver;
        }

        public override void Update(UpdateEventArgs args)
        { }

        public override void Draw()
        {
            if (!game.State.Meta.GameOver) return;

            var bgGeo = Geometries.ConsoleBackground;
            var txtGeo = Geometries.ConsoleFont;

            bgGeo.Color = Color.Red.WithAlpha(.5f).Premultiplied;
            bgGeo.DrawRectangle(-2000 * Vector2.One, 4000 * Vector2.One);
            bgGeo.Color = Color.Black.WithAlpha(.5f).Premultiplied;
            bgGeo.DrawRectangle(new Vector2(-2000, -32), new Vector2(4000, 64));

            txtGeo.Color = Color.White;
            txtGeo.SizeCoefficient = Vector2.One;
            txtGeo.Height = 48;
            txtGeo.DrawString(Vector2.Zero, "You ded", .5f, .5f);
        }
    }
}
