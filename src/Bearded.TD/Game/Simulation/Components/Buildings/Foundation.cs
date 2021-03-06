using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;
using Extensions = Bearded.TD.Tiles.Extensions;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Buildings
{
    using Sprite = IDrawableSprite<(Vector3 Normal, Vector3 Tangent, Color Color)>;

    interface IFoundation
    {
        Unit BaseHeight { get; }
        Unit TopHeight { get; }
    }

    [Component("foundation")]
    sealed class Foundation<T> : Component<T, IFoundationParameters>, IFoundation
        where T : IGameObject, IPlacedBuilding, IPositionable
    {
        private Sprite spriteSide = null!;
        private Sprite spriteTop = null!;

        public Unit BaseHeight { get; private set; }
        public Unit TopHeight { get; private set; }

        public Foundation(IFoundationParameters parameters) : base(parameters)
        {
        }

        protected override void Initialize()
        {
            // TODO: base height should possibly be the lowest tile height or even lower to prevent gaps with terrain
            BaseHeight = Owner.Position.Z;
            TopHeight = BaseHeight + Parameters.Height;

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
            foreach (var tile in Owner.OccupiedTiles)
            {
                drawTile(tile, BaseHeight, TopHeight);
            }
        }

        private void drawTile(Tile tile, Unit z0, Unit z1)
        {
            // TODO: cache the geometry
            var center = Level
                .GetPosition(tile)
                .NumericValue;

            for (var i = 0; i < 6; i++)
            {
                drawWall(z0.NumericValue, z1.NumericValue, i, center);
            }

            drawTop(z1.NumericValue, center);

            for (var i = 0; i < 3; i++)
            {
                drawConnectionIfNeeded(tile, i, center);
            }
        }

        private void drawConnectionIfNeeded(Tile tile, int i, Vector2 center)
        {
            var direction = directions[i];
            var neighbor = tile.Neighbour(direction);

            if (!Owner.Game.Level.IsValid(neighbor))
                return;

            var height = Owner.Game.GeometryLayer[tile].DrawInfo.Height;
            var neighborHeight = Owner.Game.GeometryLayer[neighbor].DrawInfo.Height;

            // TODO: this should be represented in passability or similar, but that's currently confused by buildings being present
            if ((height - neighborHeight).Squared > Constants.Game.Navigation.MaxWalkableHeightDifference.Squared)
                return;

            var neighborBuilding = Owner.Game.BuildingLayer[neighbor];

            neighborBuilding?.GetComponents<IFoundation>().MaybeFirst().Match(
                neighborFoundation => drawConnection(i, center, neighbor, neighborFoundation)
            );
        }

        private void drawConnection(int i, Vector2 center, Tile neighbor, IFoundation neighborFoundation)
        {
            var z0 = Math.Min(BaseHeight.NumericValue, neighborFoundation.BaseHeight.NumericValue);
            var z1 = Math.Min(TopHeight.NumericValue, neighborFoundation.TopHeight.NumericValue);

            var cornerBefore = cornerVectors[i];
            var cornerAfter = cornerVectors[i + 1];

            var centerN = Level.GetPosition(neighbor).NumericValue;
            var cornerBeforeN = cornerVectors[i + 4];
            var cornerAfterN = cornerVectors[i + 3];

            drawConnectionWall(centerN, cornerBeforeN, center, cornerBefore, z0, z1);
            drawConnectionWall(center, cornerAfter, centerN, cornerAfterN, z0, z1);

            spriteSide.DrawQuad(
                (center + cornerBefore * topWidthFactor).WithZ(z1),
                (center + cornerAfter * topWidthFactor).WithZ(z1),
                (centerN + cornerAfterN * topWidthFactor).WithZ(z1),
                (centerN + cornerBeforeN * topWidthFactor).WithZ(z1),
                (Vector3.UnitZ, (cornerAfter - cornerBefore).WithZ(0), Color.White)
            );
        }

        private void drawConnectionWall(Vector2 center, Vector2 cornerVector, Vector2 center2, Vector2 cornerVector2,
            float z0, float z1)
        {
            var p0 = (center + cornerVector).WithZ(z0);
            var p1 = (center + cornerVector * topWidthFactor).WithZ(z1);
            var p2 = (center2 + cornerVector2 * topWidthFactor).WithZ(z1);
            var p3 = (center2 + cornerVector2).WithZ(z0);

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

        private void drawTop(float z1, Vector2 center)
        {
            var data = (Vector3.UnitZ, Vector3.UnitX, Color.White);

            drawTopQuad(z1, center, data, 0);
            drawTopQuad(z1, center, data, 3);
        }

        private void drawTopQuad(float z1, Vector2 center, (Vector3 Normal, Vector3 Tangent, Color Color) data, int cornerOffset)
        {
            spriteTop.DrawQuad(
                (center + cornerVectors[cornerOffset + 0] * topWidthFactor).WithZ(z1),
                (center + cornerVectors[cornerOffset + 1] * topWidthFactor).WithZ(z1),
                (center + cornerVectors[cornerOffset + 2] * topWidthFactor).WithZ(z1),
                (center + cornerVectors[cornerOffset + 3] * topWidthFactor).WithZ(z1),
                cornerUVs[cornerOffset + 0],
                cornerUVs[cornerOffset + 1],
                cornerUVs[cornerOffset + 2],
                cornerUVs[cornerOffset + 3],
                data
            );
        }

        private void drawWall(float z0, float z1, int i, Vector2 center)
        {
            var cornerBefore = cornerVectors[i];
            var cornerAfter = cornerVectors[i + 1];

            var p0 = (center + cornerBefore).WithZ(z0);
            var p1 = (center + cornerBefore * topWidthFactor).WithZ(z1);
            var p2 = (center + cornerAfter * topWidthFactor).WithZ(z1);
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

        private const float topWidthFactor = 0.8f;

        private static readonly ImmutableArray<Direction> directions =
            Extensions.Directions.Append(Direction.Right).ToImmutableArray();
        private static readonly ImmutableArray<Vector2> cornerVectors =
            directions
                .Select(d => d.CornerBefore() * HexagonSide)
                .ToImmutableArray();

        private static readonly ImmutableArray<Vector2> cornerUVs =
            cornerVectors
                .Select(c => (c + new Vector2(HexagonSide)) / HexagonDiameter)
                .Select(xy => new Vector2(xy.X, 1 - xy.Y))
                .ToImmutableArray();
    }
}
