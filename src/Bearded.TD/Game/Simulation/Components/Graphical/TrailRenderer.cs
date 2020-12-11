using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenToolkit.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Graphical
{
    static class TrailRenderer
    {
        public static void DrawTrail(
            TrailTracer trail, ISprite sprite, float width,
            Instant currentTime, TimeSpan timeOut, Color color
            )
        {
            var leftUV = new Vector2(0.5f, 0);
            var rightUV = new Vector2(0.5f, 1);

            var previous = vertexLocationsFor(trail[0], width, currentTime, timeOut, color);
            foreach (var part in trail.Skip(1))
            {
                var current = vertexLocationsFor(part, width, currentTime, timeOut, color);

                sprite.DrawQuad(
                    previous.Left, current.Left, current.Right, previous.Right,
                    leftUV, leftUV, rightUV, rightUV,
                    previous.Color, current.Color, current.Color, previous.Color
                );

                previous = current;
            }
        }

        private static (Vector3 Left, Vector3 Right, Color Color) vertexLocationsFor(
            TrailTracer.Part part, float width,
            Instant currentTime, TimeSpan timeOut,
            Color color)
        {
            var center = part.Point.NumericValue;
            var offset = (part.Normal * width).WithZ();

            var alpha = Math.Max(0, (part.Timeout - currentTime) / timeOut);

            return (
                Left: center - offset,
                Right: center + offset,
                Color: color * (float)alpha
            );
        }
    }
}
