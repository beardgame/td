using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingGhostDrawing : Component, IListener<DrawComponents>, IListener<FootprintChanged>
{
    private PositionedFootprint footprint;

    protected override void OnAdded()
    {
        Events.Subscribe<DrawComponents>(this);
        Events.Subscribe<FootprintChanged>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<DrawComponents>(this);
        Events.Unsubscribe<FootprintChanged>(this);
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(FootprintChanged @event)
    {
        footprint = @event.NewFootprint;
    }

    public void HandleEvent(DrawComponents e)
    {
        var primitiveDrawer = e.Core.Primitives;

        var preconditions = Owner.GetComponents<IBuildBuildingPrecondition>();

        var preconditionParameters = new Parameters(Owner.Game, footprint);

        var result = preconditions
            .Select(c => c.CanBuild(preconditionParameters))
            .Aggregate(Result.Valid, Result.And);

        foreach (var tile in footprint.OccupiedTiles)
        {
            var isBadTile = result.BadTiles.Contains(tile);

            var color = (isBadTile ? Color.Red : Color.Green) * 0.2f;
            drawTile(e.Core, color, tile);

            if (isBadTile)
                continue;

            foreach (var direction in Directions.All.Enumerate())
            {
                var neighbor = tile.Neighbor(direction);

                if (!footprint.OccupiedTiles.Contains(neighbor))
                    continue;

                if (result.BadTiles.Contains(neighbor))
                    continue;

                if (!result.BadEdges.Contains(TileEdge.From(tile, direction)))
                    continue;

                var p = Level.GetPosition(tile).WithZ(Owner.Position.Z).NumericValue;
                var p0 = direction.CornerBefore() * Constants.Game.World.HexagonSide;
                var p1 = direction.CornerAfter() * Constants.Game.World.HexagonSide;
                primitiveDrawer.DrawLine(p + p0.WithZ(), p + p1.WithZ(), .1f, Color.Red);
            }
        }

        if (result.IsValid)
        {
            e.Core.InGameFont.DrawLine(
                Constants.Game.GameUI.ResourcesColor,
                Owner.Position.NumericValue + 0.1f * Vector3.UnitZ,
                result.Cost.Value.ToString(),
                0.18f,
                0.5f, 0.5f);
        }
    }

    private void drawTile(CoreDrawers drawers, Color color, Tile tile)
    {
        drawers.Primitives.FillCircle(
            Level.GetPosition(tile).WithZ(Owner.Position.Z).NumericValue,
            Constants.Game.World.HexagonSide,
            color,
            6);
    }
}
