using System;
using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class FactionStatus : UIComponent
    {
        private const float fontSize = Constants.UI.FontSize;
        private const float lineHeight = Constants.UI.LineHeight;
        private const float padding = Constants.UI.BoxPadding;
        
        private readonly Faction faction;

        public FactionStatus(Bounds bounds, Faction faction) : base(bounds)
        {
            this.faction = faction;
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
            var i = 0;
            Vector2 TextPos() => textStartPos + i++ * lineOffset;
            var innerWidth = (Bounds.Width - 2 * padding) * Vector2.UnitX;

            txtGeo.Color = faction.Color;
            txtGeo.DrawString(TextPos(), "Faction status");

            txtGeo.Color = Color.White * .8f;
            txtGeo.DrawString(TextPos(), "Resources:");

            var storage = faction.Resources.CurrentResources;
            var income = faction.Resources.CurrentIncome;
            txtGeo.DrawString(
                TextPos() + innerWidth, $"{storage} {signToString(income)}{Math.Abs(income)}", 1);
        }

        private static string signToString(float num) => num < 0 ? "-" : "+";
    }
}
