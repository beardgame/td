using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Rendering;
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
        protected override void Initialize()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        private static readonly Vector2[] cornerVectors =
            Extensions.Directions.Append(Direction.Right).Select(d => d.CornerBefore() * Constants.Game.World.HexagonSide).ToArray();

        public override void Draw(CoreDrawers drawers)
        {
            var sprite = Owner.Game.Meta.Blueprints
                // TODO: don't hardcode this, specify in parameters
                .Sprites[ModAwareId.ForDefaultMod("foundations")]
                // this can remain hardcoded I think, at least for now
                // later we may want to depend on parameters for tower specific config
                .Sprites.GetSprite("side");

            // TODO: the height should be in the parameters, and z0 should possibly be the lowest tile height or even lower to prevent gaps with terrain
            var z0 = Owner.Position.Z.NumericValue;
            var z1 = z0 + 0.1f;

            foreach (var tile in Owner.OccupiedTiles)
            {
                var center = Level
                    .GetPosition(tile)
                    .NumericValue;

                for (var i = 0; i < 6; i++)
                {
                    var cornerBefore = cornerVectors[i];
                    var cornerAfter = cornerVectors[i + 1];

                    sprite.DrawQuad(
                        (center + cornerBefore).WithZ(z0),
                        (center + cornerBefore * 0.8f).WithZ(z1),
                        (center + cornerAfter * 0.8f).WithZ(z1),
                        (center + cornerAfter).WithZ(z0),
                        new Vector2(0, 0),
                        new Vector2(0, 1),
                        new Vector2(1, 1),
                        new Vector2(1, 0),
                        Color.White, Color.White, Color.White, Color.White
                    );
                }

                // TODO: use a separate sprite to read from
                var uv = new Vector2(0.5f, 0.5f);

                sprite.DrawQuad(
                    (center + cornerVectors[0] * 0.8f).WithZ(z1),
                    (center + cornerVectors[1] * 0.8f).WithZ(z1),
                    (center + cornerVectors[2] * 0.8f).WithZ(z1),
                    (center + cornerVectors[3] * 0.8f).WithZ(z1),
                    uv, uv, uv, uv,
                    Color.White, Color.White, Color.White, Color.White
                );
                sprite.DrawQuad(
                    (center + cornerVectors[3] * 0.8f).WithZ(z1),
                    (center + cornerVectors[4] * 0.8f).WithZ(z1),
                    (center + cornerVectors[5] * 0.8f).WithZ(z1),
                    (center + cornerVectors[6] * 0.8f).WithZ(z1),
                    uv, uv, uv, uv,
                    Color.White, Color.White, Color.White, Color.White
                );
            }
        }
    }
}
