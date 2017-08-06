using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.UI;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game
{
    class RayCastingTestObject : GameObject
    {
        public Position2 Position { get; set; }
        private readonly List<System.TimeSpan> measuredTimes = new List<System.TimeSpan>();   

        public override void Update(TimeSpan elapsedTime)
        {
            Position = Game.Get<Cursor>().Position;
        }

        public override void Draw(GeometryManager geometries)
        {
            const int rayCount = 5;

            var rayLength = 0.1.U();

            var geo = geometries.ConsoleBackground;
            geo.LineWidth = 0.05f;

            var start = new Position2(5, 3.3f);

            foreach (var rayIndex in Enumerable.Range(0, rayCount))
            {
                var direction = Direction2.FromDegrees(360f / rayCount * rayIndex + 50);

                shootRay(start, direction, rayLength, geo);
            }

            var measureCount = 10000;

            var timer = Stopwatch.StartNew();
            
            foreach (var rayIndex in Enumerable.Range(0, measureCount))
            {
                var direction = Direction2.FromDegrees(360f / rayCount * rayIndex + 50);
                rayLength = StaticRandom.Float(0, 15).U();

                shootRay(start, direction, rayLength);
            }

            timer.Stop();

            measuredTimes.Add(timer.Elapsed);

            Game.Meta.Logger.Info.Log
                ($"shooting {measureCount} rays took: {timer.Elapsed.TotalMilliseconds:0.000}ms, which is {measureCount / timer.Elapsed.TotalMilliseconds:0} rays/ms"
                );
            var averageTime = measuredTimes.Average(ts => ts.TotalMilliseconds);
            var n = Math.Min(100, measuredTimes.Count);
            var averageTimeLastN = measuredTimes.Skip(measuredTimes.Count - n).Average(ts => ts.TotalMilliseconds);
            Game.Meta.Logger.Info.Log($"average: {measureCount / averageTime:0} rays/ms, average last {n}: {measureCount / averageTimeLastN:0}");
        }

        private void shootRay(Position2 start, Direction2 direction, Unit rayLength, PrimitiveGeometry geo)
        {
            var result = shootRay(start, direction, rayLength);
            
            geo.Color = result.Results.IsHit ? Color.Yellow : Color.Green;
            geo.DrawLine(start.NumericValue, result.GlobalPoint.NumericValue);
        }

        private TiledRayHitResult<TileInfo> shootRay(Position2 start, Direction2 direction, Unit rayLength)
        {
            var ray = new Ray(start, direction.Vector * rayLength);

            var result = Game.Level.ShootRay(ray);
            return result;
        }
    }
}
