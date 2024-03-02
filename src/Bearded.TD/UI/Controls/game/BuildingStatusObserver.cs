using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatusObserver
{
    public static BuildingStatusObserver Create(IGameWorldOverlay overlay, SelectionManager selectionManager)
    {
        var observer = new BuildingStatusObserver(overlay);

        selectionManager.ObjectFocused += observer.onObjectFocused;
        selectionManager.ObjectUnfocused += observer.onObjectUnfocused;
        selectionManager.ObjectSelected += observer.onObjectSelected;
        selectionManager.ObjectDeselected += observer.onObjectDeselected;

        return observer;
    }

    private readonly IGameWorldOverlay overlay;
    private CurrentlyShownBuilding? currentlyShown;

    private BuildingStatusObserver(IGameWorldOverlay overlay)
    {
        this.overlay = overlay;
    }

    private void onObjectFocused(ISelectable t)
    {
        // If we have an expanded view open, just hovering over a different building doesn't close our current status.
        if (currentlyShown?.IsExpanded ?? false)
        {
            return;
        }

        if (currentlyShown != null && currentlyShown.Selected != t.Object)
        {
            resetCurrentlyShown();
        }

        show(t);
    }

    private void onObjectUnfocused(ISelectable t)
    {
        if (currentlyShown?.Selected == t.Object)
        {
            resetCurrentlyShown();
        }
    }

    private void onObjectSelected(ISelectable t)
    {
        if (currentlyShown?.Selected != t.Object)
        {
            resetCurrentlyShown();
        }
        if (currentlyShown == null)
        {
            show(t);
        }
        currentlyShown!.Status.PromoteToExpandedView();
    }

    private void onObjectDeselected(ISelectable t)
    {
        if (currentlyShown?.Selected == t.Object)
        {
            resetCurrentlyShown();
        }
    }

    private void show(ISelectable t)
    {
        var status = new BuildingStatus();
        currentlyShown = new CurrentlyShownBuilding(t.Object, status, new BuildingStatusControl(status));
        // TODO: anchor to the right edge of the building rather than its center
        overlay.AddControl(
            currentlyShown.Control,
            BuildingStatusControl.Size,
            new IGameWorldOverlay.OverlayAnchor(t.Object.Position.XY(), IGameWorldOverlay.OverlayDirection.Right));
    }

    private void resetCurrentlyShown()
    {
        if (currentlyShown is null) return;
        overlay.RemoveControl(currentlyShown.Control);
        currentlyShown = null;
    }

    private sealed record CurrentlyShownBuilding(
        GameObject Selected,
        BuildingStatus Status,
        BuildingStatusControl Control)
    {
        public bool IsExpanded => Status.ShowExpanded.Value;
    }
}
