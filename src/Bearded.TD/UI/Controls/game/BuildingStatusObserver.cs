using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

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
        currentlyShown?.Status.PromoteToExpandedView();
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
        if (!t.Object.TryGetSingleComponent<IStatusTracker>(out var statusTracker))
        {
            DebugAssert.State.IsInvalid("Selectable is missing a status tracker, cannot show overlay.");
            return;
        }

        t.Object.TryGetSingleComponent<IUpgradeSlots>(out var upgradeSlots);
        t.Object.TryGetSingleComponent<IVeterancy>(out var veterancy);

        var status = new BuildingStatus(
            overlay.RequestDispatcher,
            overlay.UIContext.Content,
            overlay.SoundScape,
            t.Object,
            statusTracker,
            upgradeSlots,
            veterancy);
        var statusControl =
            new BuildingStatusControl(status, overlay.UIContext, overlay.RequestDispatcher);
        var towerRange = TowerRangeOverlayLayer.CreateAndActivateForGameObject(
            overlay.ActiveOverlays, t.Object, RangeDrawStyle.DrawFull);
        currentlyShown = new CurrentlyShownBuilding(t.Object, status, statusControl, towerRange);
        var objectPos = t.Object.Position.XY();
        var anchorPos = new Position2(t.BoundingBox.Right.U(), objectPos.Y);
        overlay.AddControl(
            currentlyShown.Control,
            statusControl.Size,
            new IGameWorldOverlay.OverlayAnchor(anchorPos, IGameWorldOverlay.OverlayDirection.Right));
    }

    private void resetCurrentlyShown()
    {
        if (currentlyShown is null) return;
        overlay.RemoveControl(currentlyShown.Control);
        currentlyShown.Status.Dispose();
        currentlyShown.TowerRange?.Deactivate();
        currentlyShown = null;
    }

    private sealed record CurrentlyShownBuilding(
        GameObject Selected,
        BuildingStatus Status,
        BuildingStatusControl Control,
        IActiveOverlay? TowerRange)
    {
        public bool IsExpanded => Status.ShowExpanded.Value;
    }
}
