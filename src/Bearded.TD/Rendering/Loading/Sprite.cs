using System;
using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Deferred;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Loading
{
    class Sprite : ISprite
    {
        private readonly IIndexedTrianglesMeshBuilder<UVColorVertex, ushort> meshBuilder;
        private readonly UVRectangle uv;
        private readonly Vector2 halfBaseSize;

        public Vector2 BaseSize { get; }

        public Sprite(IIndexedTrianglesMeshBuilder<UVColorVertex, ushort> meshBuilder, UVRectangle uv, Vector2 baseSize)
        {
            this.meshBuilder = meshBuilder;
            this.uv = uv;
            BaseSize = baseSize;
            halfBaseSize = baseSize * 0.5f;
        }


        public void Draw(Vector3 position, Color color, float size, float angle)
        {
            var unitX = new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
            var unitY = new Vector2(-unitX.Y, unitX.X);

            var v0 = (-unitX * halfBaseSize.X * size) + (unitY * halfBaseSize.Y * size);
            var v1 = (unitX * halfBaseSize.X  * size) + (unitY * halfBaseSize.Y * size);

            var v2 = -v0;
            var v3 = -v1;

            v0 += position.Xy;
            v1 += position.Xy;
            v2 += position.Xy;
            v3 += position.Xy;

            var z = position.Z;

            meshBuilder.AddQuad(
                v(v0.WithZ(z), uv.TopLeft, color),
                v(v1.WithZ(z), uv.TopRight, color),
                v(v2.WithZ(z), uv.BottomRight, color),
                v(v3.WithZ(z), uv.BottomLeft, color)
            );
        }

        public void Draw(Vector3 position, Color color, float size)
        {
            var left = position.X - size * halfBaseSize.X;
            var right = position.X + size * halfBaseSize.X;

            var top = position.Y - size * halfBaseSize.Y;
            var bottom = position.Y + size * halfBaseSize.Y;

            var z = position.Z;

            meshBuilder.AddQuad(
                v(left, top, z, uv.TopLeft, color),
                v(right, top, z, uv.TopRight, color),
                v(right, bottom, z, uv.BottomRight, color),
                v(left, bottom, z, uv.BottomLeft, color)
            );
        }

        public void DrawQuad(
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
            Color color0, Color color1, Color color2, Color color3)
        {
            meshBuilder.AddQuad(
                v(p0, transformUV(uv0), color0),
                v(p1, transformUV(uv1), color1),
                v(p2, transformUV(uv2), color2),
                v(p3, transformUV(uv3), color3)
            );
        }

        private Vector2 transformUV(Vector2 localUV)
        {
            return Vector2.Lerp(
                Vector2.Lerp(uv.TopLeft, uv.TopRight, localUV.X),
                Vector2.Lerp(uv.BottomLeft, uv.BottomRight, localUV.X),
                localUV.Y
                );
        }

        private static UVColorVertex v(float x, float y, float z, Vector2 uv, Color color)
            => v(new Vector3(x, y, z), uv, color);

        private static UVColorVertex v(Vector3 xyz, Vector2 uv, Color color)
            => new UVColorVertex(xyz, uv, color);
    }
}
