using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.World
{
    class TileInfo
    {
        private readonly Directions validDirections;

        public Directions OpenDirections { get; private set; }
        public bool IsPassable { get; private set; }

        public TileInfo(Directions validDirections)
        {
            this.validDirections = validDirections;
            OpenDirections = validDirections;
            IsPassable = true;
        }

        public void CloseTo(Direction direction)
        {
            OpenDirections = OpenDirections.Except(direction);
        }

        public void OpenTo(Direction direction)
        {
            OpenDirections = OpenDirections.And(direction).Intersect(validDirections);
        }

        public void TogglePassability()
        {
            IsPassable = !IsPassable;
        }
    }
}
