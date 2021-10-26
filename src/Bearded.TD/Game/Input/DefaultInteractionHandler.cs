using System.Linq;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    sealed class DefaultInteractionHandler : InteractionHandler
    {
        private static readonly TimeSpan doubleClickInterval = 0.2.S();

        private ISelectable? lastSelected;
        private Instant lastSelectedTime;

        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Update(ICursorHandler cursor)
        {
            var currentFootprint = cursor.CurrentFootprint;
            if (!currentFootprint.IsValid(Game.State.Level))
            {
                return;
            }

            // TODO: make this work for multiple selectables
            var selectable = Game.State.SelectionLayer.SelectablesForTile(currentFootprint.RootTile).FirstOrDefault();
            var clicked = cursor.Click.Hit;

            if (selectable != null)
            {
                if (clicked)
                {
                    if (!doubleClick(selectable))
                    {
                        Game.SelectionManager.SelectObject(selectable);
                    }
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

        private bool doubleClick(ISelectable selectable)
        {
            var now = Game.State.Time;
            var timeSinceLastSelected = now - lastSelectedTime;

            if (lastSelected == selectable
                && timeSinceLastSelected < doubleClickInterval
                && tryDoubleClickInteraction(selectable))
            {
                return true;
            }

            lastSelected = selectable;
            lastSelectedTime = now;
            return false;
        }

        private bool tryDoubleClickInteraction(ISelectable selectable)
        {
            var manualControl = selectable.Subject.Reports.OfType<IManualControlReport>().FirstOrDefault();

            if (manualControl == null)
                return false;

            Game.SelectionManager.ResetSelection();
            Game.PlayerInput.SetInteractionHandler(new ManualControlInteractionHandler(Game, manualControl));

            return true;
        }
    }
}
