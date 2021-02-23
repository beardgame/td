using Bearded.TD.Game.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Input
{
    sealed class DefaultInteractionHandler : InteractionHandler
    {
        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Update(ICursorHandler cursor)
        {
            var currentFootprint = cursor.CurrentFootprint;
            if (!currentFootprint.IsValid(Game.State.Level))
                return;

            var clicked = cursor.Click.Hit;
            getSelectableForTile(currentFootprint.RootTile, clicked).Match(
                onValue: selectable =>
                {
                    if (clicked)
                        Game.SelectionManager.SelectObject(selectable);
                    else
                        Game.SelectionManager.FocusObject(selectable);
                },
                onNothing: () =>
                {
                    if (clicked)
                        Game.SelectionManager.ResetSelection();
                    else
                        Game.SelectionManager.ResetFocus();
                });
        }

        private Maybe<ISelectable> getSelectableForTile(Tile tile, bool forClick)
        {
            var building = Game.State.BuildingLayer.GetBuildingFor(tile);
            if (building != null) return Just<ISelectable>(building);

            // TODO: replace with a better selection layer system

            return Nothing;
        }
    }
}
