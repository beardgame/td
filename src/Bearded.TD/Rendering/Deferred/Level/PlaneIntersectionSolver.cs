using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred.Level;

static class PlaneIntersectionSolver
{
    public static Vector3 FindIntersectionBruteForce(
        List<(Vector3 P, Vector3 N)> planes,
        Vector3 guess,
        Vector3 boundingBoxMin,
        Vector3 subdivisionStep)
    {
        var constant = (Span<float>)stackalloc float[planes.Count];

        var guessError = 0f;
        for (var i = 0; i < planes.Count; i++)
        {
            constant[i] = Vector3.Dot(planes[i].N, planes[i].P);
            guessError += Math.Abs(Vector3.Dot(planes[i].N, guess) - constant[i]);
        }

        var best = guess;
        var bestError = guessError;
        for (var x = 0.0f; x < 1; x += 0.1f)
        {
            for (var y = 0.0f; y < 1; y += 0.1f)
            {
                for (var z = 0.0f; z < 1; z += 0.1f)
                {
                    var error = 0.0f;
                    var candidate = boundingBoxMin + new Vector3(x, y, z) * subdivisionStep;

                    for (var i = 0; i < planes.Count; i++)
                    {
                        var n = planes[i].N;
                        error += Math.Abs(Vector3.Dot(n, candidate) - constant[i]);
                    }

                    if (error < bestError)
                    {
                        bestError = error;
                        best = candidate;
                    }
                }
            }
        }

        return best;
    }

    public static Vector3 FindIntersectionGradientDescent(
        List<(Vector3 P, Vector3 N)> planes,
        Vector3 guess,
        Vector3 min,
        Vector3 size)
    {
        var m = planes.Count;

        var A = Matrix<double>.Build.Dense(m, 3);
        var b = Vector<double>.Build.Dense(m);

        for (var i = 0; i < m; i++)
        {
            var n = planes[i].N;
            var p = planes[i].P;
            A.SetRow(i, (double[])[n.X, n.Y, n.Z]);
            b[i] = Vector3.Dot(n, p);
        }

        Func<Vector<double>, double> objectiveFunction = x =>
        {
            var diff = A * x - b;
            return diff.DotProduct(diff);
        };

        Func<Vector<double>, Vector<double>> gradientFunction = x => 2 * A.TransposeThisAndMultiply(A * x - b);

        var objective = ObjectiveFunction.Gradient(objectiveFunction, gradientFunction);

        var initialGuess = Vector<double>.Build.Dense([guess.X, guess.Y, guess.Z]);

        var lowerBound = Vector<double>.Build.Dense([min.X, min.Y, min.Z]);
        var upperBound = Vector<double>.Build.Dense([min.X + size.X, min.Y + size.Y, min.Z + size.Z]);

        var minimizer = new BfgsBMinimizer(
            gradientTolerance: 1e-2,
            parameterTolerance: 1e-5,
            functionProgressTolerance: 1,
            maximumIterations: 100);

        var result = minimizer.FindMinimum(objective, lowerBound, upperBound, initialGuess);

        var v = result.MinimizingPoint;
        return new Vector3((float)v[0], (float)v[1], (float)v[2]);
    }
}
