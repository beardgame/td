using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.World
{
    class TileInfo
    {
        public enum Type : byte
        {
            Unknown = 0,
            Floor = 1,
            Wall = 2,
        }

        public Directions ValidDirections { get; }

        public Directions OpenDirections { get; private set; }
        public bool IsPassable => building == null && TileType == Type.Floor;

        private Building building;
        public Type TileType { get; private set; }

        public TileInfo(Directions validDirections, Type tileType)
        {
            ValidDirections = validDirections;
            OpenDirections = validDirections;
            TileType = tileType;
        }

        public void CloseTo(Direction direction)
        {
            OpenDirections = OpenDirections.Except(direction);
        }

        public void OpenTo(Direction direction)
        {
            OpenDirections = OpenDirections.And(direction).Intersect(ValidDirections);
        }

        public void SetTileType(Type tileType)
        {
            TileType = tileType;
        }

        public void SetBuilding(Building building)
        {
            this.building = building;
        }
    }
}
