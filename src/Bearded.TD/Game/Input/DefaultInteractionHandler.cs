using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Meta;

namespace Bearded.TD.Game.Input
{
    class DefaultInteractionHandler : InteractionHandler
    {
        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            var currentTile = cursor.CurrentFootprint;
            if (!currentTile.IsValid)
                return;
            var building = currentTile.RootTile.Info.PlacedBuilding;
            var clicked = cursor.Click.Hit;
            if (building == null)
            {
                if (clicked)
                    Game.SelectionManager.ResetSelection();
                else
                    Game.SelectionManager.ResetFocus();
            }
            else
            {
                if (clicked)
                    Game.SelectionManager.SelectObject(building);
                else
                    Game.SelectionManager.FocusObject(building);
            }
        }

        protected override void OnStart(ICursorHandler cursor)
        {
            base.OnStart(cursor);
            Game.State.Meta.Events.BuildingConstructionStarted += onBuildingConstructionStarted;

        }

        protected override void OnEnd(ICursorHandler cursor)
        {
            base.OnEnd(cursor);
            Game.SelectionManager.ResetSelection();
            Game.State.Meta.Events.BuildingConstructionStarted -= onBuildingConstructionStarted;
        }

        private void onBuildingConstructionStarted(BuildingPlaceholder placeholder, Building building)
        {
            if (placeholder.SelectionState == SelectionState.Selected)
            {
                Game.SelectionManager.SelectObject(building);
            }
        }
    }
}
