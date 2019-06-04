using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Workers;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Input
{
    class DefaultInteractionHandler : InteractionHandler, IListener<BuildingConstructionStarted>
    {
        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            var currentFootprint = cursor.CurrentFootprint;
            if (!currentFootprint.IsValid)
                return;
            var building = Game.State.BuildingLayer.GetBuildingFor(currentFootprint.RootTile);
            var clicked = cursor.Click.Hit;
            if (building == null)
            {
                if (clicked)
                {
                    Game.State.Enumerate<Worker>()
                        .Where(w => w.CurrentTile == currentFootprint.RootTile)
                        .FirstMaybe()
                        .Match(
                            onValue: w => Game.SelectionManager.SelectObject(w),
                            onNothing: Game.SelectionManager.ResetSelection);
                }
                else
                {
                    Game.SelectionManager.ResetFocus();
                }

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
            Game.State.Meta.Events.Subscribe(this);
        }

        protected override void OnEnd(ICursorHandler cursor)
        {
            base.OnEnd(cursor);
            Game.SelectionManager.ResetSelection();
            Game.State.Meta.Events.Unsubscribe(this);
        }

        public void HandleEvent(BuildingConstructionStarted @event)
        {
            if (@event.Placeholder.SelectionState == SelectionState.Selected)
            {
                Game.SelectionManager.SelectObject(@event.Building);
            }
        }
    }
}
