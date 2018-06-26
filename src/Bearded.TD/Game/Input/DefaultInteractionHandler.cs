using amulware.Graphics;

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

        protected override void OnEnd(ICursorHandler cursor)
        {
            Game.SelectionManager.ResetSelection();
        }
    }
}
