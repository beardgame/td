using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Overlays;

sealed class BuildingHighlighter(ActiveOverlays activeOverlays)
{
    public IHighlightedBuilding StartPersistentBuildingHighlight(GameObject obj)
    {
        if (!obj.TryGetTilePresence(out var presence) || !presence.OccupiedTiles.Any())
        {
            return NoopHighlightedBuilding.Instance;
        }

        var overlay = new HighlightedBuildingOverlay(presence);
        var activeOverlay = activeOverlays.Activate(overlay);
        return new HighlightedBuilding(activeOverlay);
    }

    private class HighlightedBuilding(IActiveOverlay overlay) : IHighlightedBuilding
    {
        public void EndHighlight()
        {
            overlay.Deactivate();
        }
    }

    private class HighlightedBuildingOverlay(ITilePresence tilePresence) : IOverlayLayer
    {
        public DrawOrder DrawOrder => DrawOrder.TowerHighlight;

        public void Draw(IOverlayDrawer context)
        {
            foreach (var tile in tilePresence.OccupiedTiles)
            {
                context.Draw(tile, OverlayBrush.TowerHighlight);
            }
        }
    }

    private class NoopHighlightedBuilding : IHighlightedBuilding
    {
        public static readonly NoopHighlightedBuilding Instance = new();

        private NoopHighlightedBuilding() { }

        public void EndHighlight() { }
    }

    public interface IHighlightedBuilding
    {
        void EndHighlight();
    }
}
