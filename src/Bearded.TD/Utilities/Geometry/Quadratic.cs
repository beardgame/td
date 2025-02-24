using System;
using OpenTK.Mathematics;

namespace Bearded.TD.Utilities.Geometry;

readonly record struct Quadratic(double QuadraticTerm, double LinearTerm, double ConstantTerm);

static class QuadraticSolver
{
    public static bool TrySolveSmallestBetween(
        this Quadratic quadratic,
        double minSolution, double maxSolution,
        out double solution
    )
    {
        solution = maxSolution;

        if (!TrySolve(quadratic, out var solution1, out var solution2))
            return false;

        var found = false;

        if (solution1 >= minSolution && solution1 < solution)
        {
            found = true;
            solution = solution1;
        }

        if (solution2 >= minSolution && solution2 < solution)
        {
            found = true;
            solution = solution2;
        }

        return found;

    }

    public static bool TrySolve(
        this Quadratic quadratic,
        out double solution1, out double solution2
    )
    {
        var (quadraticTerm, linearTerm, constantTerm) = quadratic;

        // Textbook quadratic formula
        var r = linearTerm * linearTerm - 4 * quadraticTerm * constantTerm;

        if (r < 0)
        {
            solution1 = 0;
            solution2 = 0;
            return false;
        }

        var root = Math.Sqrt(r);

        var a2 = 2 * quadraticTerm;

        solution1 = (-linearTerm - root) / a2;
        solution2 = (-linearTerm + root) / a2;

        return true;
    }
}

static class RayCastHelper
{
    public static float PreciseCollision(Ray3 ray, Sphere sphere)
    {
        var relativePosition = (Vector3d)ray.Start.NumericValue - sphere.Center.NumericValue;
        var direction = (Vector3d)ray.Direction.NumericValue;
        var targetDistance = (double)sphere.Radius.NumericValue;
        var targetDistanceSquared = targetDistance * targetDistance;
        var relativePositionSquared = relativePosition.LengthSquared;

        // objects are already touching/overlapping
        if (relativePositionSquared <= targetDistanceSquared)
            return 0;

        var quadratic = Vector3d.Dot(direction, direction);
        var linear = 2 * Vector3d.Dot(relativePosition, direction);
        var constant = relativePositionSquared - targetDistanceSquared;

        var q = new Quadratic(quadratic, linear, constant);

        if (q.TrySolveSmallestBetween(0, double.PositiveInfinity, out var t))
            return (float)t;

        // no solution
        return float.PositiveInfinity;
    }
}
