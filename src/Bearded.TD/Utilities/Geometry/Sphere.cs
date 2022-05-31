using System;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Utilities.Geometry;

readonly struct Sphere
{
    public Position3 Center { get; }
    public Unit Radius { get; }

    public Sphere(Position3 center, Unit radius)
    {
        Center = center;
        Radius = radius;
    }

    public bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal)
    {
        // calculation from https://en.wikipedia.org/wiki/Line%E2%80%93sphere_intersection
        var start = ray.Start.NumericValue;
        var offsetStart = start - Center.NumericValue;
        var dir = ray.Direction.NumericValue;
        var dirNormal = dir.NormalizedSafe();

        var dirOffsetStartDot = Vector3.Dot(dirNormal, offsetStart);

        var s = dirOffsetStartDot.Squared() - offsetStart.LengthSquared + Radius.NumericValue.Squared();

        // if s is less than 0, the solutions for f are imaginary
        // and the ray's line does not intersect the sphere
        if (s >= 0)
        {
            var f = -dirOffsetStartDot - MathF.Sqrt(s);

            if (f >= 0 && f.Squared() <= dir.LengthSquared)
            {
                rayFactor = f;
                point = ray.Start + new Difference3(dirNormal * f);
                normal = new Difference3((point - Center).NumericValue.NormalizedSafe());
                return true;
            }
        }

        rayFactor = 0;
        point = default;
        normal = default;
        return false;
    }

    public bool IsInside(Position3 point) => (Center - point).LengthSquared < Radius.Squared;
}
