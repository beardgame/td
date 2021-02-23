using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;
using Extensions = Bearded.TD.Tiles.Extensions;

namespace Bearded.TD.Game.Simulation.Components.Buildings
{
    [Component("foundation")]
    sealed class Foundation : Component<Building, IFoundationParameters>
    {
        private IDrawableSprite<(Vector3 Normal, Vector3 Tangent, Color Color)> spriteSide = null!;
        private IDrawableSprite<(Vector3 Normal, Vector3 Tangent, Color Color)> spriteTop = null!;

        public Foundation(IFoundationParameters parameters) : base(parameters)
        {
        }

        protected override void Initialize()
        {
            // TODO: don't hardcode this, specify in parameters
            var sprites = Parameters.Sprites.MakeConcreteWith(
                Owner.Game.Meta.SpriteRenderers, DeferredSprite3DVertex.Create);

            // this can remain hardcoded I think, at least for now
            // later we may want to depend on parameters for tower specific config
            spriteSide = sprites.GetSprite("side");
            spriteTop = sprites.GetSprite("top");
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            // TODO: z0 should possibly be the lowest tile height or even lower to prevent gaps with terrain
            var z0 = Owner.Position.Z.NumericValue;
            var z1 = z0 + Parameters.Height.NumericValue;

            foreach (var tile in Owner.OccupiedTiles)
            {
                drawTile(tile, z0, z1);
            }
        }

        private void drawTile(Tile tile, float z0, float z1)
        {
            // TODO: cache the geometry
            var center = Level
                .GetPosition(tile)
                .NumericValue;

            for (var i = 0; i < 6; i++)
            {
                drawWall(z0, z1, i, center);
            }

            drawTop(z1, center);
        }

        private void drawTop(float z1, Vector2 center)
        {
            var data = (Vector3.UnitZ, Vector3.UnitX, Color.White);

            spriteTop.DrawQuad(
                (center + cornerVectors[0] * 0.8f).WithZ(z1),
                (center + cornerVectors[1] * 0.8f).WithZ(z1),
                (center + cornerVectors[2] * 0.8f).WithZ(z1),
                (center + cornerVectors[3] * 0.8f).WithZ(z1),
                cornerUVs[0],
                cornerUVs[1],
                cornerUVs[2],
                cornerUVs[3],
                data
            );
            spriteTop.DrawQuad(
                (center + cornerVectors[3] * 0.8f).WithZ(z1),
                (center + cornerVectors[4] * 0.8f).WithZ(z1),
                (center + cornerVectors[5] * 0.8f).WithZ(z1),
                (center + cornerVectors[6] * 0.8f).WithZ(z1),
                cornerUVs[3],
                cornerUVs[4],
                cornerUVs[5],
                cornerUVs[6],
                data
            );
        }

        private void drawWall(float z0, float z1, int i, Vector2 center)
        {
            var cornerBefore = cornerVectors[i];
            var cornerAfter = cornerVectors[i + 1];

            var p0 = (center + cornerBefore).WithZ(z0);
            var p1 = (center + cornerBefore * 0.8f).WithZ(z1);
            var p2 = (center + cornerAfter * 0.8f).WithZ(z1);
            var p3 = (center + cornerAfter).WithZ(z0);

            var tangentX = p3 - p0;
            var tangentY = p1 - p0;
            var normal = Vector3.Cross(tangentX, tangentY);

            spriteSide.DrawQuad(
                p0, p1, p2, p3,
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                (normal, tangentX, Color.White)
            );
        }

        private static readonly Vector2[] cornerVectors =
            Extensions.Directions.Append(Direction.Right).Select(d => d.CornerBefore() * HexagonSide).ToArray();

        private static readonly Vector2[] cornerUVs =
            cornerVectors
                .Select(c => (c + new Vector2(HexagonSide)) / HexagonDiameter)
                .Select(xy => new Vector2(xy.X, 1 - xy.Y))
                .ToArray();

    }
}
