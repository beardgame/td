using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.Utilities.Math;
using OpenTK;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.Tiles.Direction;

namespace Bearded.TD.Rendering.Deferred
{
    class LevelGeometry
    {
        private static readonly Color openColor = Color.Silver;
        private static readonly Color closedColor = Color.DarkGray;

        private readonly IndexedSurface<LevelVertex> surface;

        public LevelGeometry(IndexedSurface<LevelVertex> surface)
        {
            this.surface = surface;
        }

        public void DrawTile(Vector2 position, bool isOpen, Directions openDirections)
        {
            /*   p2
             *  /n0\
             * p0--p1
             *  \n1/
             *   p3
             */

            var open0 = isOpen;
            var open1 = openDirections.Includes(Right);
            var open2 = openDirections.Includes(UpRight);
            var open3 = openDirections.Includes(DownRight);

            var p0 = position.WithZ(open0 ? 0 : 1);
            var p1 = (position + new Vector2(HexagonDistanceX, 0)).WithZ(open1 ? 0 : 1);
            var p2 = (position + new Vector2(HexagonDistanceX * 0.5f, HexagonDistanceY)).WithZ(open2 ? 0 : 1);
            var p3 = (position + new Vector2(HexagonDistanceX * 0.5f, -HexagonDistanceY)).WithZ(open3 ? 0 : 1);

            var c0 = open0 ? openColor : closedColor;
            var c1 = open1 ? openColor : closedColor;
            var c2 = open2 ? openColor : closedColor;
            var c3 = open3 ? openColor : closedColor;

            var n0 = Vector3.Cross(p1 - p0, p2 - p0).Normalized();
            var n1 = Vector3.Cross(p3 - p0, p1 - p0).Normalized();

            surface.AddTriangle(
                new LevelVertex(p0, n0, Vector2.Zero, openColor),
                new LevelVertex(p2, n0, Vector2.Zero, openColor),
                new LevelVertex(p1, n0, Vector2.Zero, openColor)
            );
            surface.AddTriangle(
                new LevelVertex(p0, n1, Vector2.Zero, openColor),
                new LevelVertex(p1, n1, Vector2.Zero, openColor),
                new LevelVertex(p3, n1, Vector2.Zero, openColor)
            );
        }
    }
}
