using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingGhostDrawing<T> : Component<T>, IDrawableComponent
    where T : IComponentOwner, IGameObject, IPositionable
{
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();
    private IFactionProvider? factionProvider;

    protected override void OnAdded()
    {
        occupiedTilesTracker.Initialize(Owner, Events);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
    }

    public override void OnRemoved()
    {
        occupiedTilesTracker.Dispose(Events);
    }

    public override void Update(TimeSpan elapsedTime) {}


    public void Draw(IComponentDrawer drawer)
    {
        var primitiveDrawer = drawer.Core.Primitives;
        var anyTileOutsideWorkerNetwork = false;

        WorkerNetwork workerNetwork = null;
        factionProvider?.Faction.TryGetBehaviorIncludingAncestors(out workerNetwork);
        foreach (var tile in occupiedTilesTracker.OccupiedTiles)
        {
            var baseColor = Color.Green;

            var tileIsOutsideWorkerNetwork = Owner.Game.Level.IsValid(tile) && !(workerNetwork?.IsInRange(tile) ?? false);
            anyTileOutsideWorkerNetwork |= tileIsOutsideWorkerNetwork;

            var isTileValidForBuilding = Owner.Game.BuildingPlacementLayer.IsTileValidForBuilding(tile);

            if (!isTileValidForBuilding)
            {
                baseColor = Color.Red;
            }
            else if (tileIsOutsideWorkerNetwork)
            {
                baseColor = Color.Orange;
            }

            var color = baseColor * 0.2f;
            drawTile(drawer.Core, color, tile);

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

        if (anyTileOutsideWorkerNetwork)
        {
            renderWorkerNetworkBorderCloseBy(new Unit(10), Color.OrangeRed);
        }
        else
        {
            renderWorkerNetworkBorderCloseBy(new Unit(5), Color.DodgerBlue);
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

    private void renderWorkerNetworkBorderCloseBy(Unit maxDistance, Color baseColor)
    {
        var maxDistanceSquared = maxDistance.Squared;

        WorkerNetwork? workerNetwork = null;
        factionProvider?.Faction.TryGetBehaviorIncludingAncestors(out workerNetwork);
        var networkBorder = TileAreaBorder.From(Owner.Game.Level, t => workerNetwork?.IsInRange(t) ?? false);

        TileAreaBorderRenderer.Render(networkBorder, Owner.Game, getLineColor);

        Color? getLineColor(Position2 point)
        {
            var alpha = 1 - (point - Owner.Position.XY()).LengthSquared / maxDistanceSquared;

            return alpha < 0 ? null : baseColor * alpha;
        }
    }
}