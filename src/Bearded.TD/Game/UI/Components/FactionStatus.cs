﻿using System;
using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using OpenTK;

namespace Bearded.TD.Game.UI.Components
{
    class FactionStatus : UIComponent
    {
        private const float fontSize = 14;
        private const float lineHeight = 16;
        private const float padding = 6;
        
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
            bgGeo.DrawRectangle(Bounds.Start(), Bounds.Size());

            txtGeo.Height = fontSize;
            txtGeo.SizeCoefficient = Vector2.One;

            var textStartPos = Bounds.Start() + Vector2.One * padding;
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
