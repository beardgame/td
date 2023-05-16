using System;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Utilities.Geometry;

readonly record struct Capsule(Position3 Point1, Position3 Point2, Unit Radius)
{
    public bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal)
    {
        var ro = ray.Start.NumericValue;
        var rd = ray.Direction.NumericValue;
        var pa = Point1.NumericValue;
        var pb = Point2.NumericValue;
        var ra = Radius.NumericValue;

        // source: https://iquilezles.org/articles/intersectors/
        var ba = pb - pa;
        var oa = ro - pa;
        var baba = Vector3.Dot(ba, ba);
        var bard = Vector3.Dot(ba, rd);
        var baoa = Vector3.Dot(ba, oa);
        var rdoa = Vector3.Dot(rd, oa);
        var oaoa = Vector3.Dot(oa, oa);
        var a = baba - bard * bard;
        var b = baba * rdoa - baoa * bard;
        var c = baba * oaoa - baoa * baoa - ra * ra * baba;
        var h = b * b - a * c;

        if (h >= 0.0)
        {
            var t = (-b - MathF.Sqrt(h)) / a;
            var y = baoa + t * bard;
            // body
            if (y > 0.0 && y < baba)
            {
                rayFactor = t;
                (point, normal) = getPointAndNormal(ro, rd, rayFactor, pa, pb, ra);
                return rayFactor <= 1;
            }
            // caps
            var oc = y <= 0.0 ? oa : ro - pb;
            b = Vector3.Dot(rd, oc);
            c = Vector3.Dot(oc, oc) - ra * ra;
            h = b * b - c;
            if (h > 0.0)
            {
                rayFactor = -b - MathF.Sqrt(h);
                (point, normal) = getPointAndNormal(ro, rd, rayFactor, pa, pb, ra);
                return rayFactor <= 1;
            }
        }

        rayFactor = 0;
        point = default;
        normal = default;
        return false;
    }

    private static (Position3 point, Difference3 normal) getPointAndNormal(
        Vector3 ray0, Vector3 rayD, float f, Vector3 a, Vector3 b, float r)
    {
        var p = ray0 + rayD * f;
        var ba = b - a;
        var pa = p - a;
        var h = Math.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0f, 1.0f);
        var normal = (pa - h * ba) / r;

        return (new Position3(p), new Difference3(normal));
    }
}
