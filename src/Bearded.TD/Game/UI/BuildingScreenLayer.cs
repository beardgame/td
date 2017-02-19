using System;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using OpenTK;

namespace Bearded.TD.Game.UI
{
    class BuildingScreenLayer : UIScreenLayer
    {
        private const float fontSize = 14;
        private const float lineHeight = 16;
        private const float padding = 6;

        private static readonly string[] actionDescriptions =
        {
            "Build wall",
            "Build triangle",
            "[null]",
            "[null]",
            "[null]",
            "[null]",
            "[null]",
            "Toggle tile type",
            "Spawn enemy",
            "[null]"
        };

        private readonly GameInstance game;

        public BuildingScreenLayer(GameInstance game, GeometryManager geometries)
            : base(geometries, 0, 1, true)
        {
            this.game = game;
        }

        public override void Update(UpdateEventArgs args)
        { }

        public override void Draw()
        {
            var bgGeo = Geometries.ConsoleBackground;
            var txtGeo = Geometries.ConsoleFont;

            bgGeo.Color = Color.Black.WithAlpha(.7f).Premultiplied;
            bgGeo.DrawRectangle(Vector2.Zero, new Vector2(160, actionDescriptions.Length * lineHeight + 2 * padding));

            txtGeo.Height = fontSize;
            txtGeo.SizeCoefficient = Vector2.One;

            for (var i = 0; i < actionDescriptions.Length; i++)
            {
                txtGeo.Color = game.SelectedClickHandler == i ? Color.Yellow : Color.White;
                txtGeo.DrawString(new Vector2(padding, padding + i * lineHeight),
                    $"{(i + 1) % 10}: {actionDescriptions[i]}");
            }
        }
    }
}
