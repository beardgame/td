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

        protected override void OnDelete()
        {
            base.OnDelete();
            CurrentTile.Info.RemoveEnemy(this);
        }

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

        protected override void OnTileChange(Tile<TileInfo> oldTile, Tile<TileInfo> newTile)
        {
            base.OnTileChange(oldTile, newTile);
            if (oldTile.IsValid)
                oldTile.Info.RemoveEnemy(this);
            if (newTile.IsValid)
                newTile.Info.AddEnemy(this);
        }

        protected override void OnKill()
        {
            Game.Resources.AddBeardedPoints(Blueprint.Value);
        }
    }
}
