using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using Extensions = Bearded.TD.Tiles.Extensions;

namespace Bearded.TD.Game.Simulation.Components.Buildings
{
    [Component("foundation")]
    sealed class Foundation : Component<Building>
    {
        private IDrawableSprite<(Vector3 Normal, Vector3 Tangent, Color Color)> sprite = null!;

        protected override void Initialize()
        {
            sprite = Owner.Game.Meta.Blueprints
                // TODO: don't hardcode this, specify in parameters
                .Sprites[ModAwareId.ForDefaultMod("foundations")]
                // this can remain hardcoded I think, at least for now
                // later we may want to depend on parameters for tower specific config
                .GetSprite("side")
                .MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, DeferredSprite3DVertex.Create);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            // TODO: the height should be in the parameters, and z0 should possibly be the lowest tile height or even lower to prevent gaps with terrain
            var z0 = Owner.Position.Z.NumericValue;
            var z1 = z0 + 0.1f;

            foreach (var tile in Owner.OccupiedTiles)
            {
                drawTile(tile, z0, z1);
            }
        }

        private void drawTile(Tile tile, float z0, float z1)
        {
            var center = Level
                .GetPosition(tile)
                .NumericValue;

            for (var i = 0; i < 6; i++)
            {
                drawWall(z0, z1, i, center);
            }

            // TODO: use a separate sprite to read from
            var uv = new Vector2(0.5f, 0.5f);

            var data = (Vector3.UnitZ, Vector3.UnitX, Color.White);

            sprite.DrawQuad(
                (center + cornerVectors[0] * 0.8f).WithZ(z1),
                (center + cornerVectors[1] * 0.8f).WithZ(z1),
                (center + cornerVectors[2] * 0.8f).WithZ(z1),
                (center + cornerVectors[3] * 0.8f).WithZ(z1),
                uv, uv, uv, uv,
                data
            );
            sprite.DrawQuad(
                (center + cornerVectors[3] * 0.8f).WithZ(z1),
                (center + cornerVectors[4] * 0.8f).WithZ(z1),
                (center + cornerVectors[5] * 0.8f).WithZ(z1),
                (center + cornerVectors[6] * 0.8f).WithZ(z1),
                uv, uv, uv, uv,
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

            sprite.DrawQuad(
                p0, p1, p2, p3,
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                (normal, tangentX, Color.White)
            );
        }

        private static readonly Vector2[] cornerVectors =
            Extensions.Directions.Append(Direction.Right).Select(d => d.CornerBefore() * Constants.Game.World.HexagonSide).ToArray();

    }
}
