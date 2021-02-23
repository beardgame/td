using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    class BuildingGhost : BuildingBase<BuildingGhost>
    {
        private const string selectionIsImmutable = "Selection state of building ghost cannot be changed.";

        public BuildingGhost(IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
        }

        public void SetFootprint(PositionedFootprint footprint) => ChangeFootprint(footprint);

        public override SelectionState SelectionState => SelectionState.Selected;

        public override void ResetSelection()
            => throw new InvalidOperationException(selectionIsImmutable);
        public override void Focus()
            => throw new InvalidOperationException(selectionIsImmutable);
        public override void Select()
            => throw new InvalidOperationException(selectionIsImmutable);

        protected override IEnumerable<IComponent<BuildingGhost>> InitializeComponents()
            => Blueprint.GetComponentsForGhost();

        public override void Draw(CoreDrawers drawers)
        {
            var primitiveDrawer = drawers.Primitives;
            var anyTileOutsideWorkerNetwork = false;

            var workerNetwork = Faction.WorkerNetwork;
            foreach (var tile in Footprint.OccupiedTiles)
            {
                var baseColor = Color.Green;

                var tileIsOutsideWorkerNetwork = Game.Level.IsValid(tile) && !workerNetwork.IsInRange(tile);
                anyTileOutsideWorkerNetwork |= tileIsOutsideWorkerNetwork;

                var isTileValidForBuilding = Game.BuildingPlacementLayer.IsTileValidForBuilding(tile);

                if (!isTileValidForBuilding)
                {
                    baseColor = Color.Red;
                }
                else if (tileIsOutsideWorkerNetwork)
                {
                    baseColor = Color.Orange;
                }

                var color = baseColor * 0.5f;
                DrawTile(drawers, color, tile);

                if (!isTileValidForBuilding)
                    continue;

                foreach (var direction in Directions.All.Enumerate())
                {
                    var neighbor = tile.Neighbour(direction);

                    if (!Footprint.OccupiedTiles.Contains(neighbor))
                        continue;

                    if (!Game.BuildingPlacementLayer.IsTileValidForBuilding(neighbor))
                        continue;

                    if (Game.BuildingPlacementLayer.IsTileCombinationValidForBuilding(tile, neighbor))
                        continue;

                    var p = Level.GetPosition(tile).WithZ(Position.Z).NumericValue;
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

            base.Draw(drawers);
        }

        private void renderWorkerNetworkBorderCloseBy(Unit maxDistance, Color baseColor)
        {
            var maxDistanceSquared = maxDistance.Squared;

            var workerNetwork = Faction.WorkerNetwork;
            var networkBorder = TileAreaBorder.From(Game.Level, workerNetwork.IsInRange);

            TileAreaBorderRenderer.Render(Game, networkBorder, getLineColor);

            Color? getLineColor(Position2 point)
            {
                var alpha = 1 - (point - Position.XY()).LengthSquared / maxDistanceSquared;

                return alpha < 0 ? (Color?)null : baseColor * alpha;
            }
        }
    }
}
