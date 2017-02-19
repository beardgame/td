using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings.Components
{
    class Turret : Component
    {
        private static readonly TimeSpan ShootInterval = new TimeSpan(0.3);
        private static readonly float Range = 5;

        private Instant nextPossibleShootTime;
        private List<Tile<TileInfo>> tilesInRage;

        protected override void Initialise()
        {
            var level = Building.Game.Level;
            var position = Building.Position;
            var tile = level.GetTile(position);

            var rangeRadius = (int)Range + 1;
            var rangeSquared = Range.U().Squared;

            tilesInRage = level.Tilemap
                .SpiralCenteredAt(tile, rangeRadius)
                .Where(t => (level.GetPosition(t) - position).LengthSquared < rangeSquared)
                .ToList();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (nextPossibleShootTime <= Building.Game.Time)
            {
                // TODO
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.LineWidth = 0.05f;
            geo.Color = Color.Red * 0.5f;

            geo.DrawCircle(Building.Position.NumericValue, Range, false);
        }
    }
}