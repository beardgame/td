using System;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Footprints.events;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Selection.events;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    [Component("selectable")]
    sealed class Selectable<T> : Component<T>, ISelectable, IListener<TileEntered>, IListener<TileLeft>
        where T : IGameObject, IReportSubject
    {
        private SelectionLayer? selectionLayer;
        private SelectionState selectionState;

        public IReportSubject Subject => Owner;

        protected override void Initialize()
        {
            selectionLayer = Owner.Game.SelectionLayer;
            var acc = new AccumulateOccupiedTiles.Accumulator();
            Events.Send(new AccumulateOccupiedTiles(acc));
            acc.ToTileSet().ForEach(t => selectionLayer.RegisterSelectable(t, this));
        }

        public void ResetSelection()
        {
            var oldState = selectionState;
            selectionState = SelectionState.Default;

            switch (oldState)
            {
                case SelectionState.Focused:
                    Events.Send(new ObjectFocusReset());
                    break;
                case SelectionState.Selected:
                    Events.Send(new ObjectSelectionReset());
                    break;
                case SelectionState.Default:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Focus()
        {
            selectionState = SelectionState.Focused;
            Events.Send(new ObjectFocused());
        }

        public void Select()
        {
            selectionState = SelectionState.Selected;
            Events.Send(new ObjectSelected());
        }

        public void HandleEvent(TileEntered @event)
        {
            selectionLayer?.RegisterSelectable(@event.Tile, this);
        }

        public void HandleEvent(TileLeft @event)
        {
            selectionLayer?.UnregisterSelectable(@event.Tile, this);
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
