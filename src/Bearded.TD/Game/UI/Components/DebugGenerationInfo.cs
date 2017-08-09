using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using OpenTK;

namespace Bearded.TD.Game.UI.Components
{
    class DebugGenerationInfo : FocusableUIComponent
    {
        private readonly GameInstance game;

        public DebugGenerationInfo(Bounds bounds, GameInstance game) : base(bounds)
        {
            this.game = game;
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!IsFocused) return;
            var bgGeo = geometries.ConsoleBackground;
            bgGeo.Color = Color.Black * .7f;
            bgGeo.DrawRectangle(Bounds.Start(), Bounds.Size());

            drawText(geometries, game.Controller.DebugParameters);
        }

        private void drawText(GeometryManager geometries, GameControllerDebugParameters debugParams)
        {
            const float lineHeight = Constants.UI.LineHeight;

            var textGeo = geometries.ConsoleFont;
            textGeo.Color = Color.White;
            textGeo.SizeCoefficient = Vector2.One;
            textGeo.Height = Constants.UI.FontSize;

            var start = Bounds.Start() + Constants.UI.BoxPadding * Vector2.One;
            var i = 0;

            textGeo.DrawString(
                start + i++ * lineHeight * Vector2.UnitY, $"debit: {debugParams.Debit:0.00}");
            textGeo.DrawString(
                start + i++ * lineHeight * Vector2.UnitY, $"payoff factor: {debugParams.PayoffFactor:0.00}");
            textGeo.DrawString(
                start + i++ * lineHeight * Vector2.UnitY, $"min wave cost: {debugParams.MinWaveCost:0.00}");
            textGeo.DrawString(
                start + i++ * lineHeight * Vector2.UnitY, $"max wave cost: {debugParams.MaxWaveCost:0.00}");
            textGeo.DrawString(
                start + i * lineHeight * Vector2.UnitY, $"generation lag: {debugParams.Lag:0.00}s");
        }
    }
}
