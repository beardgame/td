using Bearded.TD.Game.Tilemap;

namespace Bearded.TD.Game.World
{
    class TileInfo
    {
        public Directions ValidDirections { get; }

        public Directions OpenDirections { get; private set; }
        public bool IsPassable { get; private set; }

        public TileInfo(Directions validDirections)
        {
            ValidDirections = validDirections;
            OpenDirections = validDirections;
            IsPassable = true;
        }

        public void CloseTo(Direction direction)
        {
            OpenDirections = OpenDirections.Except(direction);
        }

        public void OpenTo(Direction direction)
        {
            OpenDirections = OpenDirections.And(direction).Intersect(ValidDirections);
        }

        public void TogglePassability()
        {
            IsPassable = !IsPassable;
        }
    }
}
