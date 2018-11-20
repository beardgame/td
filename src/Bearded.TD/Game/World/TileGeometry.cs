namespace Bearded.TD.Game.World
{
    struct TileGeometry
    {
        public enum TileType : byte
        {
            Unknown = 0,
            Floor = 1,
            Wall = 2,
            Crevice = 3,
        }
        
        public TileType Type { get; private set; }
        public TileDrawInfo DrawInfo { get; private set; }
        
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