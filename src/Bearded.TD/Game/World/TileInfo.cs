using Bearded.TD.Game.Tilemap;

namespace Bearded.TD.Game.World
{
    class TileInfo
    {
        private readonly Directions validDirections;

        public Directions OpenDirections { get; private set; }

        public TileInfo(Directions validDirections)
        {
            this.validDirections = validDirections;
            OpenDirections = validDirections;
        }

        public void CloseTo(Direction direction)
        {
            OpenDirections = OpenDirections.Except(direction);
        }

        public void OpenTo(Direction direction)
        {
            OpenDirections = OpenDirections.And(direction).Intersect(validDirections);
        }

    }
}