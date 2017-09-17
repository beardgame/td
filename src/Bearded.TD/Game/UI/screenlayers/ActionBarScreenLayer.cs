using System.Collections.Generic;
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
            buildingPage[0] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["wall"]), "Wall");
            buildingPage[1] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["triangle"]), "Triangle");
            buildingPage[2] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["diamond"]), "Diamond");
            buildingPage[3] = contentFor(new BuildingInteractionHandler(game, faction, blueprints.Buildings["line"]), "Line");
            pages.Add(buildingPage);

#if DEBUG
            var debugPage = new ActionBarItem.Content[Constants.Game.UI.ActionBarSize];
            debugPage[0] = contentFor(new DebugSetTileTypeInteractionHandler(game, TileInfo.Type.Floor), "Make floor");
            debugPage[1] = contentFor(new DebugSetTileTypeInteractionHandler(game, TileInfo.Type.Wall), "Make wall");
            debugPage[2] = contentFor(new DebugSetTileTypeInteractionHandler(game, TileInfo.Type.Crevice), "Make crevice");
            pages.Add(debugPage);

            var enemyPage = new ActionBarItem.Content[Constants.Game.UI.ActionBarSize];
            enemyPage[0] = contentFor(new DebugSpawnEnemyInteractionHandler(game, "debug"), "Default enemy");
            enemyPage[1] = contentFor(new DebugSpawnEnemyInteractionHandler(game, "fast"), "Fast enemy");
            enemyPage[2] = contentFor(new DebugSpawnEnemyInteractionHandler(game, "strong"), "Strong enemy");
            enemyPage[3] = contentFor(new DebugSpawnEnemyInteractionHandler(game, "tank"), "Tank enemy");
            pages.Add(enemyPage);
#endif

            return pages;
        }

        private ActionBarItem.Content contentFor(InteractionHandler interactionHandler, string description)
            => new ActionBarItem.Content(
                () => game.PlayerInput.SetInteractionHandler(interactionHandler),
                () => game.PlayerInput.ResetInteractionHandler(),
                description);
    }
}
