using System;
using System.Linq;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Selection;

[Component("selectable")]
sealed class Selectable :
    Component,
    ISelectable,
    IListener<ObjectDeleting>
{
    private readonly Disposer disposer = new();
    private IVisibility? visibility;
    private SelectionLayer? selectionLayer;
    private SelectionState selectionState;
    private SelectionManager.UndoDelegate? resetFunc;
    private ITilePresenceListener? tilePresenceListener;

    public bool IsSelectable => visibility?.Visibility.IsVisible() ?? true;
    public IReportSubject Subject =>
        Owner.GetComponents<IReportSubject>().SingleOrDefault() ?? new EmptyReportSubject();

    protected override void OnAdded()
    {
        Events.Subscribe(this);

        disposer.AddDisposable(ComponentDependencies.Depend<IVisibility>(Owner, Events, v => visibility = v));
    }

    public override void Activate()
    {
        base.Activate();

        selectionLayer = Owner.Game.SelectionLayer;
        tilePresenceListener = Owner.GetTilePresence().ObserveChanges(registerTile, unregisterTile);
    }

    public override void OnRemoved()
    {
        tilePresenceListener?.Detach();
        Events.Unsubscribe(this);
        disposer.Dispose();

        base.OnRemoved();
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
        tilePresenceListener?.Detach();
        tilePresenceListener = null;
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

    public override void Update(TimeSpan elapsedTime) {}
}
