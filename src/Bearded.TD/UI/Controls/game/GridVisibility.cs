using System;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation;
using Bearded.TD.UI.Shortcuts;
using Bearded.TD.Utilities;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
    private IPositionable? anchor;
    public Binding<VisibilityMode> Visibility { get; } = new(VisibilityMode.None);

    public ShortcutLayer Shortcuts { get; }

    public GridVisibility()
    {
        Shortcuts = ShortcutLayer.CreateBuilder()
            .AddShortcut(Keys.G, () => Visibility.SetFromControl(Visibility.Value.Next()))
            .Build();
    }

    // ReSharper disable once ParameterHidesMember
    public void Initialize(ActiveOverlays activeOverlays, GameState gameState)
    {
        this.activeOverlays = activeOverlays;
        gridOverlayLayer = new GridOverlayLayer(gameState);
        buildableAreaOverlayLayer = new BuildableAreaOverlayLayer(gameState);
        Visibility.SourceUpdated += onVisibilityUpdated;
        Visibility.ControlUpdated += onVisibilityUpdated;
    }

    public void OnStartBuilding(IPositionable? maskAnchor)
    {
        anchor = maskAnchor;
        isBuilding = true;
        updateOverlays(Visibility.Value);
    }

    public void OnEndBuilding()
    {
        anchor = null;
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
            var mask = anchor == null ? null : new FadedCircleMask(anchor, 4, 3);

            activeOverlay = activeOverlays.Activate(overlayLayer, mask);
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
