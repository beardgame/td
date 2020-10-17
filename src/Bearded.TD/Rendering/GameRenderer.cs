using System;
using amulware.Graphics;
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
        }

        public void Render()
        {
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
            var shapes = geometries.Primitives;
            shapes.LineWidth = HexagonSide * 0.1f;

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

                        shapes.Color = Color.Green;
                        shapes.DrawLine(p + v, p + v + v);
                    }

                    shapes.Color = passability.IsPassable ? Color.Green : Color.Red;

                    shapes.DrawCircle(p, HexagonSide * 0.25f, true, 3);
                }
            }
        }

        private void drawDebugLevelGeometry()
        {
            var font = geometries.ConsoleFont;
            font.Height = .3f * HexagonSide;
            font.Color = Color.Orange;

            var shapes = geometries.Primitives;

            var geometryLayer = game.State.GeometryLayer;

            foreach (var tile in Tilemap.GetOutwardSpiralForTilemapWith(game.State.Level.Radius))
            {
                var p = Level.GetPosition(tile).NumericValue;
                var tileGeometry = geometryLayer[tile];
                var height = tileGeometry.Geometry.FloorHeight.NumericValue;
                var hardness = tileGeometry.Geometry.Hardness;

                shapes.Color = Color.Lerp(Color.Green, Color.Red,
                    (float) (UserSettings.Instance.Debug.LevelGeometryShowHeights ? (height + 0.5f) : hardness))
                    * 0.75f;
                shapes.DrawCircle(p.WithZ(height), HexagonSide, true, 6);

                if (UserSettings.Instance.Debug.LevelGeometryLabels)
                {
                    font.DrawString(p.WithZ(height),
                        $"{height:#.#}",
                        .5f, 1f);

                    font.DrawString(p.WithZ(height),
                        $"{hardness:#.#}",
                        .5f, 0f);
                }
            }
        }

        private void drawDebugFluids()
        {
            var geo = geometries.Primitives;
            geo.LineWidth = 0.02f;

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
                geo.Color = Color.DodgerBlue * alpha;
                geo.DrawCircle(tilePos.WithZ(numericFluidLevel + groundHeight.NumericValue), HexagonSide, true, 6);
            }

            geo.Color = Color.Aquamarine * 1f;

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
                var f = (float) flow.NumericValue;

                if (f == 0f) return;

                f = Math.Sign(f) * Math.Abs(f).Sqrted() * 5;

                var (p, q) = f > 0 ? (0f, f) : (1 + f, 1);

                var d = otherP - tileP;

                geo.DrawLine(tileP + d * p, tileP + d * q);
            }
        }

        private void drawDebugLevelMetadata()
        {
            var geo = geometries.Primitives;
            geo.LineWidth = .1f;
            foreach (var segment in game.LevelDebugMetadata.Segments)
            {
                geo.Color = segment.Color;
                geo.DrawLine(segment.From.NumericValue, segment.To.NumericValue);
            }
        }

        private void drawDebugCoordinates()
        {
            var geo = geometries.ConsoleFont;
            geo.Height = .3f * HexagonSide;
            geo.Color = Color.Beige;

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
                        geo.DrawString(topLeft, "x", .5f, .5f);
                        geo.DrawString(bottom, "y", .5f, .5f);
                        geo.DrawString(topRight, "z", .5f, .5f);
                    }
                    else
                    {
                        geo.DrawString(topLeft, $"{x:+0;-0;0}", .5f, .5f);
                        geo.DrawString(bottom, $"{y:+0;-0;0}", .5f, .5f);
                        geo.DrawString(topRight, $"{z:+0;-0;0}", .5f, .5f);
                    }
                }
                else
                {
                    geo.DrawString(Level.GetPosition(tile).NumericValue, $"{tile.X}, {tile.Y}", .5f, .5f);
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
