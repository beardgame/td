using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    sealed class UnitWarning : GameObject, IIdable<UnitWarning>
    {
        public Id<UnitWarning> Id { get; }

        private readonly Tile tile;
        private Instant nextIndicatorSpawn;

        public UnitWarning(Id<UnitWarning> id, Tile tile)
        {
            Id = id;
            this.tile = tile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            Game.IdAs(this);
            nextIndicatorSpawn = Game.Time;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Game.Time > nextIndicatorSpawn)
            {
                Game.Add(new EnemyPathIndicator(tile));
                nextIndicatorSpawn = Game.Time + Constants.Game.Enemy.TimeBetweenIndicators;
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            var radius = .3f * (1.2f + Mathf.Sin((float)Game.Time.NumericValue * Mathf.Pi));
            geo.Color = Color.Red * .8f;
            geo.LineWidth = .1f;
            var position = Game.Level.GetPosition(tile);
            geo.DrawCircle(position.NumericValue, radius, false);
        }
    }
}
