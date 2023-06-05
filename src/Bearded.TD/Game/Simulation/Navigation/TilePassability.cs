using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Navigation;

readonly record struct TilePassability(bool IsPassable, Directions PassableDirections)
{
    public TilePassability WithPassability(bool isPassable) => this with { IsPassable = isPassable };

    public TilePassability WithPassableDirections(Directions passableDirections) =>
        this with { PassableDirections = passableDirections };
}
