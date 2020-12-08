namespace Bearded.TD.Game.GameState.World
{
    struct DrawableTileGeometry
    {
        public TileGeometry Geometry { get; }
        public TileDrawInfo DrawInfo { get; }

        public TileType Type => Geometry.Type;

        public bool HasKnownType => Type != TileType.Unknown;

        public DrawableTileGeometry(TileGeometry geometry, TileDrawInfo drawInfo)
        {
            Geometry = geometry;
            DrawInfo = drawInfo;
        }

        public DrawableTileGeometry WithDrawInfo(TileDrawInfo drawInfo)
        {
            return new DrawableTileGeometry(Geometry, drawInfo);
        }
    }
}
