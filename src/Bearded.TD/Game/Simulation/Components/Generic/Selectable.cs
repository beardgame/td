using System;
using System.Collections.Generic;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    [Component("selectable")]
    sealed class Selectable<T> :
        Component<T>,
        ISelectable,
        IListener<TileEntered>,
        IListener<TileLeft>,
        IListener<ObjectDeleting>
        where T : IGameObject, IReportSubject
    {
        private SelectionLayer? selectionLayer;
        private SelectionState selectionState;
        private readonly HashSet<Tile> registeredTiles = new();
        private SelectionManager.UndoDelegate? resetFunc;

        public IReportSubject Subject => Owner;

        protected override void Initialize()
        {
            selectionLayer = Owner.Game.SelectionLayer;
            registerOccupiedTiles();

            Events.Subscribe<TileEntered>(this);
            Events.Subscribe<TileLeft>(this);
            Events.Subscribe<ObjectDeleting>(this);
        }

        private void registerOccupiedTiles()
        {
            registeredTiles.Clear();

            var acc = new AccumulateOccupiedTiles.Accumulator();
            Events.Send(new AccumulateOccupiedTiles(acc));
            acc.ToTileSet().ForEach(registerTile);
        }

        public void ResetSelection()
        {
            var oldState = selectionState;
            selectionState = SelectionState.Default;
            resetFunc = null;

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

        public void Focus(SelectionManager.UndoDelegate undoFocus)
        {
            selectionState = SelectionState.Focused;
            resetFunc = undoFocus;
            Events.Send(new ObjectFocused());
        }

        public void Select(SelectionManager.UndoDelegate undoSelection)
        {
            selectionState = SelectionState.Selected;
            resetFunc = undoSelection;
            Events.Send(new ObjectSelected());
        }

        public void HandleEvent(TileEntered @event)
        {
            registerTile(@event.Tile);
        }

        public void HandleEvent(TileLeft @event)
        {
            unregisterTile(@event.Tile);
        }

        public void HandleEvent(ObjectDeleting @event)
        {
            unregisterAllTiles();
            resetFunc?.Invoke();
        }

        private void registerTile(Tile tile)
        {
            if (registeredTiles.Add(tile))
            {
                selectionLayer?.RegisterSelectable(tile, this);
            }
        }

        private void unregisterTile(Tile tile)
        {
            if (registeredTiles.Remove(tile))
            {
                selectionLayer?.UnregisterSelectable(tile, this);
            }
        }

        private void unregisterAllTiles()
        {
            foreach (var t in registeredTiles)
            {
                selectionLayer?.UnregisterSelectable(t, this);
            }

            registeredTiles.Clear();
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
