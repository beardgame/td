namespace Bearded.TD.Game.Simulation.World;

readonly record struct DrawableTileGeometry(TileGeometry Geometry, TileDrawInfo DrawInfo)
{
    public TileType Type => Geometry.Type;

    public bool HasKnownType => Type != TileType.Unknown;
}
