namespace Bearded.TD.Game.World
{
    struct TileInfo
    {
        public TileType Type { get; }
        public TileDrawInfo DrawInfo { get; }

        public bool HasKnownType => Type != TileType.Unknown;

        public TileInfo(TileType type, TileDrawInfo drawInfo)
        {
            Type = type;
            DrawInfo = drawInfo;
        }

        public TileInfo WithDrawInfo(TileDrawInfo drawInfo)
        {
            return new TileInfo(Type, drawInfo);
        }
    }
}
