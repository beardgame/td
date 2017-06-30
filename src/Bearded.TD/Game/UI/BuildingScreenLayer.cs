﻿using amulware.Graphics;
using Bearded.TD.Game.Blueprints;
using Bearded.TD.Game.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Game.UI
{
    class BuildingScreenLayer : UIScreenLayer
    {
        private const float fontSize = Constants.UI.FontSize;
        private const float lineHeight = Constants.UI.LineHeight;
        private const float padding = Constants.UI.BoxPadding;

        private readonly GameInstance game;
        private readonly IClickHandler[] clickHandlers;
        private int selectedHandler = -1;

        public BuildingScreenLayer(ScreenLayerCollection parent, GameInstance game, GeometryManager geometries)
            : base(parent, geometries)
        {
            this.game = game;

            clickHandlers = initializeClickHandlers(game.Me.Faction, game.Blueprints);
        }

        protected override bool DoHandleInput(InputContext input)
        {
            for (var i = 0; i < clickHandlers.Length; i++)
            {
                if (!input.Manager.IsKeyHit(clickHandlerKeys[i])) continue;
                if (i == selectedHandler)
                {
                    game.Cursor.SetClickHandler(null);
                    selectedHandler = -1;
                }
                else
                {
                    game.Cursor.SetClickHandler(clickHandlers[i]);
                    selectedHandler = i;
                }
                break;
            }

            return true;
        }

        public override void Update(UpdateEventArgs args)
        { }

        public override void Draw()
        {
            var bgGeo = Geometries.ConsoleBackground;
            var txtGeo = Geometries.ConsoleFont;

            bgGeo.Color = Color.Black * 0.7f;
            bgGeo.DrawRectangle(
                Vector2.Zero,
                new Vector2(180, actionDescriptions.Length * lineHeight + 2 * padding));

            txtGeo.Height = fontSize;
            txtGeo.SizeCoefficient = Vector2.One;

            for (var i = 0; i < actionDescriptions.Length; i++)
            {
                txtGeo.Color = selectedHandler == i ? Color.Yellow : Color.White;
                txtGeo.DrawString(new Vector2(padding, padding + i * lineHeight),
                    $"{(i + 1) % 10}: {actionDescriptions[i]}");
            }
        }

        #region Definitions

        private static IClickHandler[] initializeClickHandlers(Faction faction, BlueprintManager blueprints)
        {
            return new IClickHandler[]
            {
                new BuildingClickHandler(faction, blueprints.Buildings["wall"]), // 1
                new BuildingClickHandler(faction, blueprints.Buildings["triangle"]), // 2
                null, // 3
                null, // 4
                null, // 5
                null, // 6
                null, // 7
                new DebugToggleTileTypeClickHandler(), // 8
                new DebugSpawnEnemyClickHandler(), // 9
                null // 0
            };
        }

        private static readonly Key[] clickHandlerKeys =
        {
            Key.Number1, Key.Number2, Key.Number3, Key.Number4, Key.Number5,
            Key.Number6, Key.Number7, Key.Number8, Key.Number9, Key.Number0
        };

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
        #endregion
    }
}
