using System;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.Rendering.Loading
{
    class Sprite : ISprite
    {
        private readonly IndexedSurface<UVColorVertexData> surface;
        private readonly UVRectangle uv;
        private readonly Vector2 halfBaseSize;

        private readonly Vector2 uvBase;
        private readonly Vector2 uUnit;
        private readonly Vector2 vUnit;

        public Vector2 BaseSize { get; }

        public Sprite(IndexedSurface<UVColorVertexData> surface, UVRectangle uv, Vector2 baseSize)
        {
            this.surface = surface;
            this.uv = uv;
            BaseSize = baseSize;
            halfBaseSize = baseSize * 0.5f;

            uvBase = uv.TopLeft;
            uUnit = uv.TopRight - uvBase;
            vUnit = uv.BottomLeft - uvBase;
        }


        public void Draw(Vector3 position, Color color, float size, float angle)
        {
            var unitX = new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
            var unitY = new Vector2(-unitX.Y, unitX.X);

            //var scaledUnitX =

            var v0 = (-unitX * halfBaseSize.X * size) + (unitY * halfBaseSize.Y * size);
            var v1 = (unitX * halfBaseSize.X  * size) + (unitY * halfBaseSize.Y * size);

            var v2 = -v0;
            var v3 = -v1;

            v0 += position.Xy;
            v1 += position.Xy;
            v2 += position.Xy;
            v3 += position.Xy;

            var z = position.Z;

            surface.AddQuad(
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

            surface.AddQuad(
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
            surface.AddQuad(
                v(p0, transformUV(uv0), color0),
                v(p1, transformUV(uv1), color1),
                v(p2, transformUV(uv2), color2),
                v(p3, transformUV(uv3), color3)
            );
        }

        private Vector2 transformUV(Vector2 localUV)
            => uvBase + localUV.X * uUnit + localUV.Y * vUnit;

        private static UVColorVertexData v(Vector3 xyz, Vector2 uv, Color color)
            => new UVColorVertexData(xyz, uv, color);

        private static UVColorVertexData v(float x, float y, float z, Vector2 uv, Color color)
            => new UVColorVertexData(x, y, z, uv, color);
    }
}
