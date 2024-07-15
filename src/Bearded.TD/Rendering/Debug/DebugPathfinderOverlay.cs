using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Shapes;
using Bearded.Utilities;

namespace Bearded.TD.Rendering.Debug;

sealed class DebugPathfinderOverlay(GameInstance game, CoreDrawers drawers) : IOverlayLayer
{
    public DrawOrder DrawOrder => DrawOrder.Debug;

    private IActiveOverlay? receipt;

    private Tile pathfinderDebugTarget;
    private IIterativePathfinder? debugPathFinder;
    private DateTime nextPathfinderDebugStep;
    private Pathfinder.Result? result;

    public void UpdateAndDrawTextIf(bool visible)
    {
        if (!visible)
        {
            receipt?.Deactivate();
            receipt = null;
            return;
        }
        receipt ??= game.Overlays.Activate(this);

        var cursor = game.PlayerInput.CursorPosition;
        var cursorTile = Level.GetTile(cursor);
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
                    Color.Red, p, line,
                    fontHeight: 0.5f
                );
                p.Y -= 0.5f;
            }
        }

        var (argb, text) =
            success
                ? result == null
                    ? (Color.Yellow, $"no path found in {timer.Elapsed.TotalMilliseconds:0.00}ms")
                    : (Color.Lime, $"path found in {timer.Elapsed.TotalMilliseconds:0.00}ms")
                : (Color.Red, $"pathfinder crashed after {timer.Elapsed.TotalMilliseconds:0.00}ms");

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
        var debugLines = new List<string>();

        if (result != null)
        {
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
                    Color.Aqua, p, line,
                    fontHeight: 0.5f, alignVertical: 1
                );
                p.Y -= 0.5f;
            }
        }
    }

    public void Draw(IOverlayDrawer context)
    {
        if (debugPathFinder == null)
            return;

        var brushes = (
            Seen: new OverlayBrush(Fill.With(Color.Yellow * 0.25f)),
            Open: new OverlayBrush(Fill.With(Color.Blue * 0.25f)),
            NextOpen: new OverlayBrush(Fill.With(Color.Red * 0.5f)),
            Path: new OverlayBrush(Fill.With(Color.Lime * 0.5f))
        );

        var debugState = debugPathFinder.GetCurrentState();

        foreach (var tile in debugState.SeenTiles.Distinct())
        {
            context.Draw(tile, brushes.Seen);
        }

        foreach (var tile in debugState.OpenTiles.Distinct())
        {
            context.Draw(tile, new(Fill.With(Color.Blue * 0.25f)));
        }

        foreach (var tile in debugState.NextOpenTiles)
        {
            context.Draw(tile, new(Fill.With(Color.Red * 0.5f)));
        }

        if (result != null)
        {
            var tile = Tile.Origin;

            foreach (var direction in result.Path)
            {
                tile = tile.Neighbor(direction);
                context.Draw(tile, new(Fill.With(Color.Lime * 0.5f)));
            }
        }
    }
}
