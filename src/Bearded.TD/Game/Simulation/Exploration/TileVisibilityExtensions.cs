namespace Bearded.TD.Game.Simulation.Exploration
{
    static class TileVisibilityExtensions
    {
        public static bool IsRevealed(this TileVisibility visibility) =>
            visibility is TileVisibility.Revealed or TileVisibility.Visible;

        public static bool IsVisible(this TileVisibility visibility) => visibility == TileVisibility.Visible;
    }
}
