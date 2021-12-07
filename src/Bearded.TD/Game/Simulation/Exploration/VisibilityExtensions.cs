namespace Bearded.TD.Game.Simulation.Exploration
{
    static class VisibilityExtensions
    {
        public static bool IsVisible(this ObjectVisibility visibility) => visibility == ObjectVisibility.Visible;

        public static bool IsRevealed(this TileVisibility visibility) =>
            visibility is TileVisibility.Revealed or TileVisibility.Visible;

        public static bool IsVisible(this TileVisibility visibility) => visibility == TileVisibility.Visible;

        public static bool IsRevealed(this ZoneVisibility visibility) => visibility == ZoneVisibility.Revealed;
    }
}
