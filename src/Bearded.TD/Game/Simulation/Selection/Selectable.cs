using System;
using System.Linq;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Selection
{
    [Component("selectable")]
    sealed class Selectable<T> :
        Component<T>,
        ISelectable,
        IListener<ObjectDeleting>
        where T : IComponentOwner, IGameObject
    {
        private SelectionLayer? selectionLayer;
        private SelectionState selectionState;
        private readonly OccupiedTilesTracker occupiedTilesTracker = new();
        private SelectionManager.UndoDelegate? resetFunc;

        public IReportSubject Subject =>
            Owner.GetComponents<IReportSubject>().SingleOrDefault() ?? new EmptyReportSubject();

        protected override void Initialize()
        {
            selectionLayer = Owner.Game.SelectionLayer;
            occupiedTilesTracker.Initialize(Owner, Events);
            occupiedTilesTracker.OccupiedTiles.ForEach(registerTile);
            occupiedTilesTracker.TileAdded += registerTile;
            occupiedTilesTracker.TileRemoved += unregisterTile;
            Events.Subscribe(this);
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

        public void HandleEvent(ObjectDeleting @event)
        {
            unregisterAllTiles();
            resetFunc?.Invoke();
        }

        private void registerTile(Tile tile)
        {
            selectionLayer?.RegisterSelectable(tile, this);
        }

        private void unregisterTile(Tile tile)
        {
            selectionLayer?.UnregisterSelectable(tile, this);
        }

        private void unregisterAllTiles()
        {
            foreach (var t in occupiedTilesTracker.OccupiedTiles)
            {
                selectionLayer?.UnregisterSelectable(t, this);
            }
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
