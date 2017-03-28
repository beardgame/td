using System;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Game.UI
{
    class BuildingScreenLayer : UIScreenLayer
    {
        private const float fontSize = 14;
        private const float lineHeight = 16;
        private const float padding = 6;

        private readonly GameInstance game;
        private int selectedHandler = -1;

        public BuildingScreenLayer(ScreenLayerCollection parent, GameInstance game, GeometryManager geometries)
            : base(parent, geometries, 0, 1, true)
        {
            this.game = game;
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            for (var i = 0; i < clickHandlers.Length; i++)
            {
                if (!InputManager.IsKeyHit(clickHandlerKeys[i])) continue;
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

            bgGeo.Color = Color.Black.WithAlpha(.7f).Premultiplied;
            bgGeo.DrawRectangle(
                Vector2.Zero,
                new Vector2(160, (1 + actionDescriptions.Length) * lineHeight + 2 * padding));

            txtGeo.Height = fontSize;
            txtGeo.SizeCoefficient = Vector2.One;

            for (var i = 0; i < actionDescriptions.Length; i++)
            {
                txtGeo.Color = selectedHandler == i ? Color.Yellow : Color.White;
                txtGeo.DrawString(new Vector2(padding, padding + i * lineHeight),
                    $"{(i + 1) % 10}: {actionDescriptions[i]}");
            }

            txtGeo.Color = Color.Beige;
            txtGeo.DrawString(
                new Vector2(padding, padding + actionDescriptions.Length * lineHeight),
                $"Resources: {game.State.Resources.CurrentResources}");
        }

        #region Definitions
        private static readonly IClickHandler[] clickHandlers = {
            new BuildingClickHandler(new BuildingBlueprint(TileSelection.Single, 100, 5, null)), // 1
            new BuildingClickHandler(new BuildingBlueprint(
                TileSelection.Triangle, 300, 20, new Func<Component>[] { () => new Turret() })
            ), // 2
            null, // 3
            null, // 4
            null, // 5
            null, // 6
            null, // 7
            new DebugToggleTileTypeClickHandler(), // 8
            new DebugSpawnEnemyClickHandler(), // 9
            null // 0
        };

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
