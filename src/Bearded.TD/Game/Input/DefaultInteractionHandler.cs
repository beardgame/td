using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Workers;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Input
{
    sealed class DefaultInteractionHandler : InteractionHandler
    {
        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            var currentFootprint = cursor.CurrentFootprint;
            if (!currentFootprint.IsValid)
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

            // The following calculations may expensive and if we can't focus these on hover anyway, might as well skip.
            if (!forClick) return Nothing;

            return Game.State.Enumerate<Worker>()
                .Where(w => w.CurrentTile == tile)
                .Cast<ISelectable>()
                .MaybeFirst();
        }
    }
}
