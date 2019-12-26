using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    struct TileGeometry
    {
        public TileType Type { get; }
        public double Hardness { get; }
        public Unit FloorHeight { get; }

        public TileGeometry(TileType type, double hardness, Unit floorHeight)
        {
            Type = type;
            Hardness = hardness;
            FloorHeight = floorHeight;
        }
    }
}
