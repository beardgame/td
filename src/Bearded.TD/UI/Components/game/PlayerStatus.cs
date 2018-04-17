using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Collections;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    sealed class PlayerStatus : UIComponent
    {
        private const float fontSize = Constants.UI.FontSize;
        private const float lineHeight = Constants.UI.LineHeight;
        private const float padding = Constants.UI.BoxPadding;

        private readonly GameInstance game;

        public PlayerStatus(Bounds bounds, GameInstance game) : base(bounds)
        {
            this.game = game;
        }

        public override void Draw(GeometryManager geometries)
        {
            var bgGeo = geometries.ConsoleBackground;
            var txtGeo = geometries.ConsoleFont;

            bgGeo.Color = Color.Black * .7f;
            bgGeo.DrawRectangle(Bounds.TopLeft(), Bounds.Size());

            txtGeo.Height = fontSize;
            txtGeo.SizeCoefficient = Vector2.One;

            var textStartPos = Bounds.TopLeft() + Vector2.One * padding;
            var lineOffset = lineHeight * Vector2.UnitY;
            Vector2 TextPos(int k) => textStartPos + k * lineOffset;
            var innerWidth = (Bounds.Width - 2 * padding) * Vector2.UnitX;

            foreach (var (player, i) in game.Players.Indexed())
            {
                txtGeo.Color = player.Faction.Color;
                txtGeo.DrawString(TextPos(i), player.Name);
                txtGeo.DrawString(TextPos(i) + innerWidth * Vector2.UnitX, $"{player.LastKnownPing:N0}", 1);
            }
        }
    }
}
