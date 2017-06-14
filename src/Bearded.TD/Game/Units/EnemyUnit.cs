using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Units
{
    class EnemyUnit : GameUnit
    {
        private bool dealtDamage;

        public EnemyUnit(Id<GameUnit> unitId, UnitBlueprint blueprint, Tile<TileInfo> currentTile)
            : base(unitId, blueprint, currentTile)
        { }

        protected override void OnDelete()
        {
            base.OnDelete();
            CurrentTile.Info.RemoveEnemy(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (!dealtDamage)
                tryDealDamage();
        }

        private void tryDealDamage()
        {
            var target = CurrentTile.Neighbours
                .Select(t => t.Info.Building).FirstOrDefault(b => b != null && b.HasComponentOfType<EnemySink>());
            if (target == null)
                return;
            target.Damage(Blueprint.Damage);
            dealtDamage = true;
            this.Sync(KillUnit.Command, this, (Faction) null);
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.DarkRed;
            geo.DrawRectangle(Position.NumericValue - Vector2.One * .25f, Vector2.One * .5f);

            var p = Health / (float)Blueprint.Health;
            geo.Color = Color.DarkGray;
            geo.DrawRectangle(Position.NumericValue - new Vector2(0.5f), new Vector2(1, 0.1f));
            geo.Color = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), 0.8f, 0.8f);
            geo.DrawRectangle(Position.NumericValue - new Vector2(0.5f), new Vector2(1 * p, 0.1f));
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
