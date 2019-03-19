namespace Bearded.TD.Game.World
{
    struct TileGeometry
    {
        public TileType Type { get; }
        public TileDrawInfo DrawInfo { get; }

        public bool HasKnownType => Type != TileType.Unknown;

        public TileGeometry(TileType type, TileDrawInfo drawInfo)
        {
            Type = type;
            DrawInfo = drawInfo;
        }

        public TileGeometry WithDrawInfo(TileDrawInfo drawInfo)
        {
            return new TileGeometry(Type, drawInfo);
        }
    }
}
