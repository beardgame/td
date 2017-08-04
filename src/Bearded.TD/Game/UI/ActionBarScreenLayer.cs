﻿using System.Collections.Generic;
using Bearded.TD.Game.UI.Components;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI;
using Bearded.TD.Utilities.Input;
using OpenTK;

namespace Bearded.TD.Game.UI
{
    class ActionBarScreenLayer : UIScreenLayer
    {
        private readonly GameInstance game;

        public ActionBarScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, GameInstance game, InputManager inputManager)
            : base(parent, geometries)
        {
            this.game = game;
            AddComponent(new ActionBar(
                Bounds.AnchoredBox(Screen, 0, .5f, new Vector2(120, 320)),
                Constants.Game.UI.ActionBarSize,
                getPages(),
                inputManager.Actions.Mouse.RightButton));
        }

        private List<ActionBarItem.Content[]> getPages()
        {
            var faction = game.Me.Faction;
            var blueprints = game.Blueprints;

            var pages = new List<ActionBarItem.Content[]>();

            var buildingPage = new ActionBarItem.Content[Constants.Game.UI.ActionBarSize];
            buildingPage[0] = contentFor(new BuildingClickHandler(faction, blueprints.Buildings["wall"]), "Wall");
            buildingPage[1] = contentFor(new BuildingClickHandler(faction, blueprints.Buildings["triangle"]), "Triangle");
            pages.Add(buildingPage);

#if DEBUG
            var debugPage = new ActionBarItem.Content[Constants.Game.UI.ActionBarSize];
            debugPage[0] = contentFor(new DebugToggleTileTypeClickHandler(), "Toggle tile");
            debugPage[1] = contentFor(new DebugSpawnEnemyClickHandler(), "Spawn enemy");
            debugPage[2] = contentFor(new DebugSetTileTypeClickHandler(TileInfo.Type.Crevice), "Dig Too Deep");
            pages.Add(debugPage);
#endif

            return pages;
        }

        private ActionBarItem.Content contentFor(IClickHandler clickHandler, string description)
            => new ActionBarItem.Content(
                () => game.Cursor.SetClickHandler(clickHandler),
                () => game.Cursor.SetClickHandler(null),
                description);
    }
}
