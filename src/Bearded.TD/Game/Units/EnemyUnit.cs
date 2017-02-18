using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.Game.Units
{
    class EnemyUnit : GameUnit
    {
        public EnemyUnit(UnitBlueprint blueprint, Tile<TileInfo> currentTile) : base(blueprint, currentTile)
        { }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.DarkRed;
            geo.DrawRectangle(Position.NumericValue - Vector2.One * .25f, Vector2.One * .5f);
        }

        protected override Direction GetNextDirection()
        {
            return Game.Navigator.GetDirections(CurrentTile);
        }
    }
}
