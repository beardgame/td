namespace Bearded.TD.Game.World
{
    struct TileGeometry
    {
        public TileType Type { get; }
        public double Hardness { get; }

        public TileGeometry(TileType type, double hardness)
        {
            Type = type;
            Hardness = hardness;
        }
    }
}
