using System;
using amulware.Graphics;
using amulware.Graphics.Shapes;
using amulware.Graphics.Text;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Meta;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenToolkit.Mathematics;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering
{
    sealed class GameRenderer
    {
        private readonly GameInstance game;
        private readonly GeometryManager geometries;
        private readonly LevelRenderer levelRenderer;
        private readonly FluidGeometry waterGeometry;

        private readonly IShapeDrawer2<Color> shapeDrawer;
        private readonly TextDrawerWithDefaults<Color> debugGeometryTextDrawer;
        private readonly TextDrawerWithDefaults<Color> debugCoordinateTextDrawer;

        public ContentSurfaceManager DeferredSurfaces { get; }

        public GameRenderer(GameInstance game, RenderContext renderContext)
        {
            this.game = game;
            geometries = renderContext.Geometries;

            // TODO: this should not stay hardcoded forever
            var levelMaterial = game.Blueprints.Materials[ModAwareId.ForDefaultMod("default")];
            var waterMaterial = game.Blueprints.Materials[ModAwareId.ForDefaultMod("water")];

            levelRenderer = new GPUHeightmapLevelRenderer(game, renderContext, levelMaterial);

            waterGeometry = new FluidGeometry(game, game.State.FluidLayer.Water, renderContext, waterMaterial);

            DeferredSurfaces = new ContentSurfaceManager(
                renderContext,
                levelRenderer,
                game.Blueprints.Sprites,
                new [] { waterGeometry }
            );

            shapeDrawer = geometries.Primitives;
            debugGeometryTextDrawer = geometries.ConsoleFont.With(
                fontHeight: .3f * HexagonSide, parameters: Color.Orange, alignHorizontal: .5f);
            debugCoordinateTextDrawer = geometries.ConsoleFont.With(
                fontHeight: .3f * HexagonSide, parameters: Color.Beige, alignHorizontal: .5f, alignVertical: .5f);
        }

        public void Render()
        {
            // TODO: clean mesh builder so we can draw again!

            game.PlayerCursors.DrawCursors(geometries);
            drawAmbientLight();
            drawGameObjects();
            drawDebug();
        }

        private void drawAmbientLight()
        {
            var radius = game.State.Level.Radius;

            geometries.PointLight.Draw(
                new Vector3(-radius * 2, radius * 2, radius),
                radius * 10, Color.White * 0.2f
            );
        }

        private void drawGameObjects()
        {
            foreach (var obj in game.State.GameObjects)
            {
                obj.Draw(geometries);
            }
        }

        private void drawDebug()
        {
            var settings = UserSettings.Instance.Debug;

            if (settings.LevelGeometry)
            {
                drawDebugLevelGeometry();
            }

            if (settings.Passability)
            {
                drawDebugPassabilityLayer();
            }

            if (settings.Coordinates > 0)
            {
                drawDebugCoordinates();
            }

            var debugPathfinding = settings.Pathfinding;
            if (debugPathfinding > 0)
            {
                game.State.Navigator.DrawDebug(geometries, debugPathfinding > 1);
            }

            if (settings.SimpleFluid)
            {
                drawDebugFluids();
            }

            if (settings.LevelMetadata)
            {
                drawDebugLevelMetadata();
            }
        }

        private void drawDebugPassabilityLayer()
        {
            const float lineWidth = HexagonSide * 0.1f;

            var passabilityLayer = game.State.PassabilityManager.GetLayer(Passability.WalkingUnit);

            foreach (var tile in Tilemap.GetOutwardSpiralForTilemapWith(game.State.Level.Radius))
            {
                var p = Level.GetPosition(tile).NumericValue;

                var passability = passabilityLayer[tile];

                foreach (var direction in Directions.All.Enumerate())
                {
                    if (passability.PassableDirections.Includes(direction))
                    {
                        var v = direction.Vector() * HexagonWidth * 0.5f;

                        shapeDrawer.DrawLine(p + v, p + v + v, lineWidth, Color.Green);
                    }

                    shapeDrawer.FillCircle(p, HexagonSide * 0.25f, passability.IsPassable ? Color.Green : Color.Red, 3);
                }
            }
        }

        private void drawDebugLevelGeometry()
        {
            var geometryLayer = game.State.GeometryLayer;

            foreach (var tile in Tilemap.GetOutwardSpiralForTilemapWith(game.State.Level.Radius))
            {
                var p = Level.GetPosition(tile).NumericValue;
                var tileGeometry = geometryLayer[tile];
                var height = tileGeometry.Geometry.FloorHeight.NumericValue;
                var hardness = tileGeometry.Geometry.Hardness;

                var color = Color.Lerp(Color.Green, Color.Red,
                    (float) (UserSettings.Instance.Debug.LevelGeometryShowHeights ? (height + 0.5f) : hardness))
                    * 0.75f;
                shapeDrawer.FillCircle(p.WithZ(height), HexagonSide, color, 6);

                if (UserSettings.Instance.Debug.LevelGeometryLabels)
                {
                    debugGeometryTextDrawer.DrawLine(
                        xyz: p.WithZ(height),
                        text: $"{height:#.#}",
                        alignVertical: 1f);

                    debugGeometryTextDrawer.DrawLine(
                        xyz: p.WithZ(height),
                        text: $"{hardness:#.#}",
                        alignVertical: 0f);
                }
            }
        }

        private void drawDebugFluids()
        {
            var water = game.State.FluidLayer.Water;
            var ground = game.State.GeometryLayer;

            foreach (var tile in Tilemap.EnumerateTilemapWith(game.State.Level.Radius))
            {
                var (fluidLevel, _) = water[tile];

                if (fluidLevel.NumericValue <= 0.0001)
                    continue;

                var tilePos = Level.GetPosition(tile).NumericValue;

                var groundHeight = ground[tile].DrawInfo.Height;

                var numericFluidLevel = (float) fluidLevel.NumericValue;

                var alpha = Math.Min(numericFluidLevel * 10, 0.5f);
                shapeDrawer.FillCircle(
                    tilePos.WithZ(numericFluidLevel + groundHeight.NumericValue),
                    HexagonSide,
                    Color.DodgerBlue * alpha,
                    6);
            }

            foreach (var tile in Tilemap.EnumerateTilemapWith(game.State.Level.Radius))
            {
                var (_, flow) = water[tile];

                var tilePos = Level.GetPosition(tile).NumericValue;

                drawFlow(tilePos, Level.GetPosition(tile.Neighbour(Direction.Right)).NumericValue, flow.FlowRight);
                drawFlow(tilePos, Level.GetPosition(tile.Neighbour(Direction.UpRight)).NumericValue, flow.FlowUpRight);
                drawFlow(tilePos, Level.GetPosition(tile.Neighbour(Direction.UpLeft)).NumericValue, flow.FlowUpLeft);
            }

            void drawFlow(Vector2 tileP, Vector2 otherP, FlowRate flow)
            {
                const float lineWidth = 0.02f;

                var f = (float) flow.NumericValue;

                if (f == 0f) return;

                f = Math.Sign(f) * Math.Abs(f).Sqrted() * 5;

                var (p, q) = f > 0 ? (0f, f) : (1 + f, 1);

                var d = otherP - tileP;

                shapeDrawer.DrawLine(tileP + d * p, tileP + d * q, lineWidth, Color.Aquamarine * 1f);
            }
        }

        private void drawDebugLevelMetadata()
        {
            foreach (var segment in game.LevelDebugMetadata.Segments)
            {
                shapeDrawer.DrawLine(segment.From.NumericValue, segment.To.NumericValue, .1f, segment.Color);
            }
        }

        private void drawDebugCoordinates()
        {
            var topLeftOffset = Direction2.FromDegrees(150).Vector * HexagonSide * .6f;
            var bottomOffset = Direction2.FromDegrees(270).Vector * HexagonSide * .6f;
            var topRightOffset = Direction2.FromDegrees(30).Vector * HexagonSide * .6f;

            foreach (var tile in Tilemap.EnumerateTilemapWith(game.State.Level.Radius))
            {
                if (UserSettings.Instance.Debug.Coordinates > 1)
                {
                    var (x, y, z) = tile.ToXYZ();

                    var tilePos = Level.GetPosition(tile).NumericValue;
                    var topLeft = tilePos + topLeftOffset;
                    var bottom = tilePos + bottomOffset;
                    var topRight = tilePos + topRightOffset;

                    if (x == 0 && y == 0 && z == 0)
                    {
                        debugCoordinateTextDrawer.DrawLine(topLeft.WithZ(), text: "x");
                        debugCoordinateTextDrawer.DrawLine(bottom.WithZ(), "y");
                        debugCoordinateTextDrawer.DrawLine(topRight.WithZ(), "z");
                    }
                    else
                    {
                        debugCoordinateTextDrawer.DrawLine(topLeft.WithZ(), $"{x:+0;-0;0}");
                        debugCoordinateTextDrawer.DrawLine(bottom.WithZ(), $"{y:+0;-0;0}");
                        debugCoordinateTextDrawer.DrawLine(topRight.WithZ(), $"{z:+0;-0;0}");
                    }
                }
                else
                {
                    debugCoordinateTextDrawer.DrawLine(
                        Level.GetPosition(tile).NumericValue.WithZ(), $"{tile.X}, {tile.Y}");
                }
            }
        }

        public void CleanUp()
        {
            levelRenderer.CleanUp();
            waterGeometry.CleanUp();
            DeferredSurfaces.Dispose();
            GraphicsUnloader.CleanUp(game.Blueprints);
        }

    }
}
