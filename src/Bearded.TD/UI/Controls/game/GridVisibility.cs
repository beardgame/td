using System;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Utilities;
using static Bearded.TD.UI.Controls.GridVisibility;

namespace Bearded.TD.UI.Controls;

sealed class GridVisibility
{
    private ActiveOverlays activeOverlays = null!;
    private GridOverlayLayer gridOverlayLayer = null!;
    private BuildableAreaOverlayLayer buildableAreaOverlayLayer = null!;

    private IActiveOverlay? activeGridOverlay;
    private IActiveOverlay? activeBuildableAreaOverlay;

    private bool isBuilding;
    public Binding<VisibilityMode> Visibility { get; } = new(VisibilityMode.None);

    // ReSharper disable once ParameterHidesMember
    public void Initialize(ActiveOverlays activeOverlays, GameState gameState)
    {
        this.activeOverlays = activeOverlays;
        gridOverlayLayer = new GridOverlayLayer(gameState);
        buildableAreaOverlayLayer = new BuildableAreaOverlayLayer(gameState);
        Visibility.SourceUpdated += onVisibilityUpdated;
        Visibility.ControlUpdated += onVisibilityUpdated;
    }

    public void OnStartBuilding()
    {
        isBuilding = true;
        updateOverlays(Visibility.Value);
    }

    public void OnEndBuilding()
    {
        isBuilding = false;
        updateOverlays(Visibility.Value);
    }

    private void onVisibilityUpdated(VisibilityMode newVisibility)
    {
        updateOverlays(newVisibility);
    }

    private void updateOverlays(VisibilityMode visibility)
    {
        updateOverlay(
            isBuilding || visibility.ShowsGrid(), ref activeGridOverlay, gridOverlayLayer);
        updateOverlay(
            isBuilding || visibility.ShowsBuildableArea(), ref activeBuildableAreaOverlay, buildableAreaOverlayLayer);
    }

    private void updateOverlay(bool expectedVisibility, ref IActiveOverlay? activeOverlay, IOverlayLayer overlayLayer)
    {
        var currentVisibility = activeOverlay is not null;
        if (currentVisibility == expectedVisibility) return;
        if (expectedVisibility)
        {
            activeOverlay = activeOverlays.Activate(overlayLayer);
        }
        else
        {
            activeOverlay!.Deactivate();
            activeOverlay = null;
        }
    }

    public enum VisibilityMode
    {
        None,
        GridOnly,
        GridAndBuildableArea,
    }
}

static class VisibilityModeExtensions
{
    public static VisibilityMode Next(this VisibilityMode v) =>
        (VisibilityMode) (((int) v + 1) % Enum.GetValues<VisibilityMode>().Length);

    public static bool ShowsGrid(this VisibilityMode v) => v != VisibilityMode.None;

    public static bool ShowsBuildableArea(this VisibilityMode v) => v == VisibilityMode.GridAndBuildableArea;
}
