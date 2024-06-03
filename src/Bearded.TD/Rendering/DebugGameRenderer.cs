using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.Graphics.Text;
using Bearded.TD.Game;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TileVisibility = Bearded.TD.Game.Simulation.Exploration.TileVisibility;
using TimeSpan = System.TimeSpan;

namespace Bearded.TD.Rendering;

using static Color;
using static Bearded.TD.Constants.Game.World;

sealed class DebugGameRenderer(GameInstance game, RenderContext context)
{
    private readonly CoreDrawers drawers = context.Drawers;
    private readonly IShapeDrawer2<Color> shapeDrawer = context.Drawers.Primitives;

    private readonly TextDrawerWithDefaults<Color> debugGeometryTextDrawer = context.Drawers.InGameFont
        .With(fontHeight: .3f * HexagonSide, parameters: Orange, alignHorizontal: .5f);

    private readonly TextDrawerWithDefaults<Color> debugCoordinateTextDrawer = context.Drawers.InGameFont
        .With(fontHeight: .3f * HexagonSide, parameters: Beige, alignHorizontal: .5f, alignVertical: .5f);

    private Tile pathfinderDebugTarget;
    private IIterativePathfinder? debugPathFinder;
    private DateTime nextPathfinderDebugStep;

    public void Draw()
    {
        var settings = UserSettings.Instance.Debug;

        if (settings.Zones)
        {
            drawDebugZones();
        }

        if (settings.Visibility)
        {
            drawDebugVisibility();
        }

        if (settings.Biomes)
        {
            drawDebugBiomes();
        }

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
            game.State.Navigator.DrawDebug(drawers, debugPathfinding > 1);
        }

        if (settings.SimpleFluid)
        {
            drawDebugFluids();
        }

        if (settings.LevelMetadata)
        {
            drawDebugLevelMetadata();
        }

        if (settings.DebugPathfinder > 0)
        {
            debugPathfinder();
        }
    }

    private void drawDebugPassabilityLayer()
    {
        const float lineWidth = HexagonSide * 0.1f;

        var passabilityLayer = game.State.PassabilityObserver.GetLayer(Passability.WalkingUnit);

        foreach (var tile in Tilemap.GetOutwardSpiralForTilemapWith(game.State.Level.Radius))
        {
            var p = Level.GetPosition(tile).NumericValue;

            var passability = passabilityLayer[tile];

            foreach (var direction in Directions.All.Enumerate())
            {
                if (passability.PassableDirections.Includes(direction))
                {
                    var v = direction.Vector() * HexagonWidth * 0.5f;

                    shapeDrawer.DrawLine(p + v, p + v + v, lineWidth, Green);
                }

                shapeDrawer.FillCircle(p, HexagonSide * 0.25f, passability.IsPassable ? Green : Red, 3);
            }
        }
    }

    private void drawDebugZones()
    {
        var zoneLayer = game.State.ZoneLayer;
        var visibilityLayer = game.State.VisibilityLayer;

        foreach (var zone in zoneLayer.AllZones)
        {
            var color = visibilityLayer[zone] switch
            {
                ZoneVisibility.Invisible => DarkOrange,
                ZoneVisibility.Revealed => LightGreen,
                _ => throw new ArgumentOutOfRangeException(),
            };

            TileAreaBorderRenderer.Render(TileAreaBorder.From(zone.CoreTiles), game.State, color);
        }
    }

    private void drawDebugVisibility()
    {
        const float a = 0.1f;

        var visibilityLayer = game.State.VisibilityLayer;

        foreach (var tile in Tilemap.GetOutwardSpiralForTilemapWith(game.State.Level.Radius))
        {
            var color = visibilityLayer[tile] switch
            {
                TileVisibility.Invisible => DarkOrange,
                TileVisibility.Revealed => YellowGreen,
                TileVisibility.Visible => LightGreen,
                _ => throw new ArgumentOutOfRangeException(),
            };

            var xyz = Level.GetPosition(tile).NumericValue;
            shapeDrawer.FillCircle(xyz, HexagonSide, color * a, 6);
        }
    }

    private void drawDebugBiomes()
    {
        const float a = 0.1f;

        var biomeLayer = game.State.BiomeLayer;

        foreach (var tile in Tilemap.GetOutwardSpiralForTilemapWith(game.State.Level.Radius))
        {
            var xyz = Level.GetPosition(tile).NumericValue;
            shapeDrawer.FillCircle(xyz, HexagonSide, biomeLayer[tile].OverlayColor * a, 6);
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

            var color = Lerp(Green, Red,
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
                DodgerBlue * alpha,
                6);
        }

        foreach (var tile in Tilemap.EnumerateTilemapWith(game.State.Level.Radius))
        {
            var (_, flow) = water[tile];

            var tilePos = Level.GetPosition(tile).NumericValue;

            drawFlow(tilePos, Level.GetPosition(tile.Neighbor(Direction.Right)).NumericValue, flow.FlowRight);
            drawFlow(tilePos, Level.GetPosition(tile.Neighbor(Direction.UpRight)).NumericValue, flow.FlowUpRight);
            drawFlow(tilePos, Level.GetPosition(tile.Neighbor(Direction.UpLeft)).NumericValue, flow.FlowUpLeft);
        }

        void drawFlow(Vector2 tileP, Vector2 otherP, FlowRate flow)
        {
            const float lineWidth = 0.02f;

            var f = (float) flow.NumericValue;

            if (f == 0f) return;

            f = Math.Sign(f) * Math.Abs(f).Sqrted() * 5;

            var (p, q) = f > 0 ? (0f, f) : (1 + f, 1);

            var d = otherP - tileP;

            shapeDrawer.DrawLine(tileP + d * p, tileP + d * q, lineWidth, Aquamarine * 1f);
        }
    }

    private void drawDebugLevelMetadata()
    {
        game.LevelDebugMetadata.Visit(data =>
        {
            switch (data)
            {
                case LevelDebugMetadata.AreaBorder border:
                    TileAreaBorderRenderer.Render(border.Border, drawers.CustomPrimitives, border.Color);
                    break;
                case LevelDebugMetadata.Circle circle:
                    shapeDrawer.DrawCircle(circle.Center.NumericValue, circle.Radius.NumericValue, circle.LineWidth.NumericValue, circle.Color);
                    break;
                case LevelDebugMetadata.Disk disk:
                    shapeDrawer.FillCircle(disk.Center.NumericValue, disk.Radius.NumericValue, disk.Color);
                    break;
                case LevelDebugMetadata.LineSegment segment:
                    shapeDrawer.DrawLine(segment.From.NumericValue, segment.To.NumericValue, segment.Width?.NumericValue ?? 0 + .1f, segment.Color);
                    break;
                case LevelDebugMetadata.Text text:
                    debugGeometryTextDrawer.DrawLine(text.Color, text.Position.WithZ(0).NumericValue, text.Value, text.FontHeight?.NumericValue, text.AlignX);
                    break;
                case LevelDebugMetadata.Tile tile:
                    var xyz = Level.GetPosition(tile.XY).WithZ(tile.Z).NumericValue;
                    shapeDrawer.FillCircle(xyz, HexagonSide, tile.Color, 6);
                    break;
            }
        });
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


    private void debugPathfinder()
    {
        var cursor = game.PlayerInput.CursorPosition;
        var cursorTile = Level.GetTile(cursor);
        Pathfinder.Result? result = null;
        var success = false;
        var timer = new Stopwatch();

        var gameState = game.State;

        double? costFunction(Tile tile, Direction step)
        {
            var neighbour = tile.Neighbor(step);
            return
                gameState.Level.IsValid(tile)
                && gameState.GeometryLayer[tile].Type == TileType.Floor
                && gameState.Level.IsValid(neighbour)
                && gameState.GeometryLayer[neighbour].Type == TileType.Floor
                    ? 1
                    : null;
        }

        var pathfinder = UserSettings.Instance.Debug.DebugPathfinder switch
        {
            1 => Pathfinder.CreateAStar(costFunction, 1),
            2 => Pathfinder.CreateBidirectionalAStar(costFunction, 1),
            _ => null
        };

        if (pathfinder == null)
        {
            return;
        }

        try
        {
            timer.Start();

            result = pathfinder.FindPath(Tile.Origin, cursorTile);

            timer.Stop();

            success = true;
        }
        catch (Exception e)
        {
            var lines = e.ToString().Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries);
            var p = cursor.NumericValue.WithZ();
            foreach (var line in lines)
            {
                drawers.InGameFont.DrawLine(
                    Red, p, line,
                    fontHeight: 0.5f
                );
                p.Y -= 0.5f;
            }
        }

        var (argb, text) =
            success
                ? result == null
                    ? (Yellow, $"no path found in {timer.Elapsed.TotalMilliseconds:0.00}ms")
                    : (Lime, $"path found in {timer.Elapsed.TotalMilliseconds:0.00}ms")
                : (Red, $"pathfinder crashed after {timer.Elapsed.TotalMilliseconds:0.00}ms");

        drawers.InGameFont.DrawLine(
            argb, cursor.NumericValue.WithZ(), text,
            fontHeight: 0.5f,
            alignVertical: 1f
        );

        if (pathfinderDebugTarget != cursorTile || debugPathFinder == null)
        {
            pathfinderDebugTarget = cursorTile;
            debugPathFinder = pathfinder.FindPathIteratively(Tile.Origin, cursorTile);
            nextPathfinderDebugStep = DateTime.Now;
        }

        if (nextPathfinderDebugStep <= DateTime.Now)
        {
            try
            {
                debugPathFinder.TryAdvanceStep();

                nextPathfinderDebugStep += TimeSpan.FromMilliseconds(50);
            }
            catch
            {
                nextPathfinderDebugStep = DateTime.MaxValue;
            }
        }

        var debugState = debugPathFinder.GetCurrentState();

        foreach (var tile in debugState.SeenTiles.Distinct())
        {
            drawers.Primitives.FillCircle(
                Level.GetPosition(tile).NumericValue, HexagonSide, Yellow * 0.25f, 6);
        }

        foreach (var tile in debugState.OpenTiles.Distinct())
        {
            drawers.Primitives.FillCircle(
                Level.GetPosition(tile).NumericValue, HexagonSide, Blue * 0.25f, 6);
        }

        foreach (var tile in debugState.NextOpenTiles)
        {
            drawers.Primitives.FillCircle(
                Level.GetPosition(tile).NumericValue, HexagonSide, Red * 0.5f, 6);
        }

        var debugLines = new List<string>();

        if (result != null)
        {
            var tile = Tile.Origin;

            var pathColor = Lime * 0.5f;

            foreach (var direction in result.Path)
            {
                tile = tile.Neighbor(direction);
                drawers.Primitives.FillCircle(
                    Level.GetPosition(tile).NumericValue, HexagonSide, pathColor, 6);
            }

            debugLines.Add($"path steps: {result.Path.Length}");
            debugLines.Add($"path cost: {result.Cost}");
        }

        var finalStateDebugPathfinder = pathfinder.FindPathIteratively(Tile.Origin, cursorTile);
        while (finalStateDebugPathfinder.TryAdvanceStep())
        {
        }

        var finalDebugState = finalStateDebugPathfinder.GetCurrentState();

        debugLines.Add($"nodes considered: {finalDebugState.SeenTiles.Count()}");
        debugLines.Add($"nodes expanded: {finalDebugState.SeenTiles.Except(finalDebugState.OpenTiles).Count()}");


        {
            var p = cursor.NumericValue.WithZ();
            p.Y += debugLines.Count * 0.5f;
            foreach (var line in debugLines)
            {
                drawers.InGameFont.DrawLine(
                    Aqua, p, line,
                    fontHeight: 0.5f, alignVertical: 1
                );
                p.Y -= 0.5f;
            }
        }
    }

}
