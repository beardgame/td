using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Units
{
    class EnemyUnit : GameUnit
    {
        public EnemyUnit(UnitBlueprint blueprint, Tile<TileInfo> currentTile) : base(blueprint, currentTile)
        { }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            foreach (var b in Game.Enumerate<Base>())
            {
                if (!b.OccupiedTiles.Any(tile => tile.DistanceTo(CurrentTile) <= 1))
                    continue;
                b.Damage(Blueprint.Damage);
                Delete();
                return;
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.DarkRed;
            geo.DrawRectangle(Position.NumericValue - Vector2.One * .25f, Vector2.One * .5f);
        }

        protected override Direction GetNextDirection()
        {
            var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
            return !CurrentTile.Neighbour(desiredDirection).Info.IsPassable
                ? Direction.Unknown
                : desiredDirection;
        }
    }
}
