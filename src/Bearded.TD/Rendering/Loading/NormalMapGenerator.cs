using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Loading;

sealed class NormalMapGenerator
{
    private const float outlineThreshold = 20 / 255f;

    private const int kernelRadius = 3;
    private static readonly ImmutableArray<float> kernel =
        ImmutableArray.Create(0.382f, 0.242f, 0.061f, 0.006f);

    private readonly SpriteTextureTransformation.Data data;

    public NormalMapGenerator(SpriteTextureTransformation.Data data)
    {
        this.data = data;
    }

    public void Generate()
    {
        var buffer1 = new float[data.Width, data.Height];
        var buffer2 = new float[data.Width, data.Height];
        var red = data.Red;
        foreachPixel((x, y) => buffer1[x, y] = red[x, y] / 255f);
        
        blurH(buffer1, buffer2);
        blurV(buffer2, buffer1);
        blurH(buffer1, buffer2);
        blurV(buffer2, buffer1);
        blurH(buffer1, buffer2);
        blurV(buffer2, buffer1);
        blurH(buffer1, buffer2);
        blurV(buffer2, buffer1);
        blurH(buffer1, buffer2);
        blurV(buffer2, buffer1);
        blurH(buffer1, buffer2);
        blurV(buffer2, buffer1);

        var final = buffer1;
        
        foreachPixel((x, y) =>
        {
            var dx = x > 0 && x < data.Width - 1
                ? final[x - 1, y] - final[x + 1, y]
                : 0;
            var dy = y > 0 && y < data.Height - 1
                ? final[x, y - 1] - final[x, y + 1]
                : 0;
            var dz = MathF.Sqrt(1 - (dx.Squared() + dy.Squared()));
            var normal = new Vector3(dx, -dy, dz * 0.05f).Normalized();
            
            var rgb = (normal + new Vector3(1)) * 0.5f * 255;
            
            data[x, y] = new Color(
                (byte)rgb.X,
                (byte)rgb.Y,
                (byte)rgb.Z,
                255);
        });
    }

    private void blurH(float[,] from, float[,] to)
    {
        transform(from, to, (x, y, input) =>
        {
            float sum, weightSum;

            {
                var value = input[x, y];
                if (value < outlineThreshold)
                {
                    return value;
                }

                var weight = kernel[0];
                sum = value * weight;
                weightSum = weight;
            }

            for (var k = 1; k <= kernelRadius; k++)
            {
                if (x + k >= data.Width)
                    break;
                var value = input[x + k, y];
                if (value < outlineThreshold)
                    break;
                var weight = kernel[k];
                sum += value * weight;
                weightSum += weight;
            }

            for (var k = 1; k <= kernelRadius; k++)
            {
                if (x - k < 0)
                    break;
                var value = input[x - k, y];
                if (value < outlineThreshold)
                    break;
                var weight = kernel[k];
                sum += value * weight;
                weightSum += weight;
            }

            return sum / weightSum;
        });
    }
    private void blurV(float[,] from, float[,] to)
    {
        transform(from, to, (x, y, input) =>
        {
            float sum, weightSum;

            {
                var value = input[x, y];
                if (value < outlineThreshold)
                {
                    return value;
                }

                var weight = kernel[0];
                sum = value * weight;
                weightSum = weight;
            }

            for (var k = 1; k <= kernelRadius; k++)
            {
                if (y + k >= data.Height)
                    break;
                var value = input[x, y + k];
                if (value < outlineThreshold)
                    break;
                var weight = kernel[k];
                sum += value * weight;
                weightSum += weight;
            }
            
            for (var k = 1; k <= kernelRadius; k++)
            {
                if (y - k < 0)
                    break;
                var value = input[x, y - k];
                if (value < outlineThreshold)
                    break;
                var weight = kernel[k];
                sum += value * weight;
                weightSum += weight;
            }

            return sum / weightSum;
        });
    }

    private void transform<TIn, TOut>(TIn[,] from, TOut[,] to, Func<int, int, TIn[,], TOut> calculatePixel)
    {
        foreach (var y in Enumerable.Range(0, data.Height))
        foreach (var x in Enumerable.Range(0, data.Width))
        {
            to[x, y] = calculatePixel(x, y, from);
        }
    }

    private void foreachPixel(Action<int, int> action)
    {
        foreach (var y in Enumerable.Range(0, data.Height))
        foreach (var x in Enumerable.Range(0, data.Width))
        {
            action(x, y);
        }
    }
}
