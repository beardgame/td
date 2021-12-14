using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Navigation;

struct TilePassability
{
    public bool IsPassable { get; }
    public Directions PassableDirections { get; }

    public TilePassability(bool isPassable, Directions passableDirections)
    {
        IsPassable = isPassable;
        PassableDirections = passableDirections;
    }

    public TilePassability WithPassability(bool isPassable)
    {
        return new TilePassability(isPassable, PassableDirections);
    }

    public TilePassability WithPassableDirections(Directions passableDirections)
    {
        return new TilePassability(IsPassable, passableDirections);
    }
}