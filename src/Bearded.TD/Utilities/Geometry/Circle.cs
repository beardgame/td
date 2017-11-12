using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.Geometry
{
    struct Circle
    {
        public Position2 Center { get; }
        public Unit Radius { get; }

        public Circle(Position2 center, Unit radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool TryHit(Ray ray, out float rayFactor, out Position2 point, out Difference2 normal)
        {
            var start = ray.Start.NumericValue;
            var dir = ray.Direction.NumericValue;

            var a = start.X - Center.X.NumericValue;
            var b = start.Y - Center.Y.NumericValue;
            var r2 = Radius.Squared.NumericValue;

            var c2 = dir.X * dir.X;
            var d2 = dir.Y * dir.Y;
            var cd = dir.X * dir.Y;

            var s = (r2 - a * a) * d2 +
                    (r2 - b * b) * c2 +
                    2 * a * b * cd;

            // if s is less than 0, the solutions for f are imaginary
            // and the ray's line does not intersect the circle
            if (s >= 0)
            {
                var f = ((float)Math.Sqrt(s) + a * dir.X + b * dir.Y) / -(c2 + d2);

                if (f <= 1)
                {
                    if (f >= 0 || (a * a + b * b < r2 && !float.IsNegativeInfinity(f)))
                    {
                        rayFactor = f;
                        point = ray.Start + ray.Direction * f;
                        normal = new Difference2(a, b);
                        return true;
                    }
                }
            }

            rayFactor = 0;
            point = new Position2();
            normal = new Difference2();
            return false;
        }

        public bool IsInside(Position2 point) => (Center - point).LengthSquared < Radius.Squared;
    }
}
