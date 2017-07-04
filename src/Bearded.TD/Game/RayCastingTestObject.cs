using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.UI;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game
{
    class RayCastingTestObject : GameObject
    {
        public Position2 Position { get; set; }

        public override void Update(TimeSpan elapsedTime)
        {
            Position = Game.Get<Cursor>().Position;
        }

        public override void Draw(GeometryManager geometries)
        {
            const int rayCount = 20;

            var rayLength = 5.U();

            var geo = geometries.ConsoleBackground;
            geo.LineWidth = 0.05f;

            var start = new Position2(-0.5f, 0);

            foreach (var rayIndex in Enumerable.Range(0, rayCount))
            {
                var direction = Direction2.FromDegrees(-90);//360f / rayCount * rayIndex);

                var ray = new Ray(start, direction.Vector * rayLength);

                try
                {
                    var result = Game.Level.ShootRay(ray);

                    geo.Color = result.Results.IsHit ? Color.Yellow : Color.Green;
                    geo.DrawLine(start.NumericValue, result.GlobalPoint.NumericValue);
                }
                catch
                {
                    geo.Color = Color.Red;
                    geo.DrawLine(start.NumericValue, (ray.Start + ray.Direction).NumericValue);
                }
            }
        }
    }
}
