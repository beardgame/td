namespace Bearded.TD.Game.Input
{
    sealed class DefaultInteractionHandler : InteractionHandler
    {
        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Update(ICursorHandler cursor)
        {
            var currentFootprint = cursor.CurrentFootprint;
            if (!currentFootprint.IsValid(Game.State.Level))
            {
                return;
            }

            var selectable = Game.State.SelectionLayer.SelectableForTile(currentFootprint.RootTile);
            var clicked = cursor.Click.Hit;

            if (selectable != null)
            {
                if (clicked)
                {
                    Game.SelectionManager.SelectObject(selectable);
                }
                else
                {
                    Game.SelectionManager.FocusObject(selectable);
                }
            }
            else
            {
                if (clicked)
                {
                    Game.SelectionManager.ResetSelection();
                }
                else
                {
                    Game.SelectionManager.ResetFocus();
                }
            }
        }
    }
}
