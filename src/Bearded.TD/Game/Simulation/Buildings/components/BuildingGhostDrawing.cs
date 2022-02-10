using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingGhostDrawing<T> : Component<T>, IListener<DrawComponents>
    where T : IComponentOwner, IGameObject, IPositionable
{
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();

    protected override void OnAdded()
    {
        occupiedTilesTracker.Initialize(Owner, Events);
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        occupiedTilesTracker.Dispose(Events);
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(DrawComponents e)
    {
        var primitiveDrawer = e.Core.Primitives;

        foreach (var tile in occupiedTilesTracker.OccupiedTiles)
        {
            var baseColor = Color.Green;

            var isTileValidForBuilding = Owner.Game.BuildingPlacementLayer.IsTileValidForBuilding(tile);

            if (!isTileValidForBuilding)
            {
                baseColor = Color.Red;
            }

            var color = baseColor * 0.2f;
            drawTile(e.Core, color, tile);

            if (!isTileValidForBuilding)
                continue;

            foreach (var direction in Directions.All.Enumerate())
            {
                var neighbor = tile.Neighbor(direction);

                if (!occupiedTilesTracker.OccupiedTiles.Contains(neighbor))
                    continue;

                if (!Owner.Game.BuildingPlacementLayer.IsTileValidForBuilding(neighbor))
                    continue;

                if (Owner.Game.BuildingPlacementLayer.IsTileCombinationValidForBuilding(tile, neighbor))
                    continue;

                var p = Level.GetPosition(tile).WithZ(Owner.Position.Z).NumericValue;
                var p0 = direction.CornerBefore() * Constants.Game.World.HexagonSide;
                var p1 = direction.CornerAfter() * Constants.Game.World.HexagonSide;
                primitiveDrawer.DrawLine(p + p0.WithZ(), p + p1.WithZ(), .1f, Color.Red);
            }
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
