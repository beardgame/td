using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Units
{
    class EnemyUnit : GameUnit
    {
        private Instant nextAttack;

        public EnemyUnit(Id<GameUnit> unitId, UnitBlueprint blueprint, Tile<TileInfo> currentTile)
            : base(unitId, blueprint, currentTile)
        { }

        protected override void OnAdded()
        {
            base.OnAdded();

            nextAttack = Game.Time + Blueprint.TimeBetweenAttacks;
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            CurrentTile.Info.RemoveEnemy(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            tryDealDamage();
        }

        private void tryDealDamage()
        {
            if (IsMoving) return;

            while (nextAttack <= Game.Time)
            {
                var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
                var target = CurrentTile.Neighbour(desiredDirection).Info.Building;

                if (target == null) return;
                
                target.Damage(Blueprint.Damage);
                nextAttack = Game.Time + Blueprint.TimeBetweenAttacks;
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Blueprint.Color;
            var size = (Mathf.Atan(.005f * (Blueprint.Health - 200)) + Mathf.PiOver2) / Mathf.Pi;
            geo.DrawRectangle(Position.NumericValue - Vector2.One * size * .5f, Vector2.One * size);

            var p = (Health / (float)Blueprint.Health).Clamped(0, 1);
            geo.Color = Color.DarkGray;
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1, .1f));
            geo.Color = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1 * p, .1f));
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

        protected override void OnKill(Faction killingBlowFaction)
        {
            killingBlowFaction?.Resources.ProvideOneTimeResource(Blueprint.Value);
        }
    }
}
