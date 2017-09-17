using System.Collections.Generic;
using Bearded.TD.Game.UI.Components;
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
            buildingPage[0] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["wall"]), "Wall");
            buildingPage[1] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["triangle"]), "Triangle");
            buildingPage[2] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["diamond"]), "Diamond");
            buildingPage[3] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["line"]), "Line");
            pages.Add(buildingPage);

#if DEBUG
            /*var debugPage = new ActionBarItem.Content[Constants.Game.UI.ActionBarSize];
            debugPage[0] = contentFor(new DebugToggleTileTypeClickHandler(), "Toggle tile");
            debugPage[1] = contentFor(new DebugSetTileTypeClickHandler(TileInfo.Type.Crevice), "Dig Too Deep");
            pages.Add(debugPage);

            var enemyPage = new ActionBarItem.Content[Constants.Game.UI.ActionBarSize];
            enemyPage[0] = contentFor(new DebugSpawnEnemyClickHandler("debug"), "Default enemy");
            enemyPage[1] = contentFor(new DebugSpawnEnemyClickHandler("fast"), "Fast enemy");
            enemyPage[2] = contentFor(new DebugSpawnEnemyClickHandler("strong"), "Strong enemy");
            enemyPage[3] = contentFor(new DebugSpawnEnemyClickHandler("tank"), "Tank enemy");
            pages.Add(enemyPage);*/
#endif

            return pages;
        }

        private ActionBarItem.Content contentFor(InteractionHandler interactionHandler, string description)
            => new ActionBarItem.Content(() => { }, () => { }, description);
    }
}
