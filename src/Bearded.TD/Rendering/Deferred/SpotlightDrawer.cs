using System;
using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Shapes;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred;

sealed class SpotlightDrawer
{
    private readonly struct Parameters
    {
        public Vector3 Center { get; }
        public Vector3 Direction { get; }
        public float Angle { get; }
        public float RadiusSquared { get; }
        public Color Color { get; }

        public Parameters(Vector3 center, Vector3 direction, float angle, float radiusSquared, Color color)
        {
            Center = center;
            Direction = direction;
            Angle = angle;
            RadiusSquared = radiusSquared;
            Color = color;
        }
    }

    private readonly ShapeDrawer3<SpotlightVertex, Parameters> drawer;

    public SpotlightDrawer(IIndexedTrianglesMeshBuilder<SpotlightVertex, ushort> meshBuilder)
    {
        drawer = new ShapeDrawer3<SpotlightVertex, Parameters>(
            meshBuilder,
            (xyz, p) => new SpotlightVertex(xyz, p.Center, p.Direction, p.Angle, p.RadiusSquared, p.Color));
    }

    public void Draw(Vector3 center, Vector3 direction, float radius, float angle, Color color)
    {
        const float innerToOuterRadius = 2 / 1.73205080757f; // R = 2 / sqrt(3) * r
        const int edges = 6;

        var vectorFromCenterToBase = direction.NormalizedSafe() * radius;
        var coneBase = center + vectorFromCenterToBase;

        var innerBaseRadius = MathF.Tan(angle) * radius;
        var outerBaseRadius = innerBaseRadius * innerToOuterRadius;

        var parameters = new Parameters(center, direction, angle, radius.Squared(), color);

        drawer.DrawCone(coneBase, -vectorFromCenterToBase, outerBaseRadius, parameters, edges);
    }
}