using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.InGameUI;
using Bearded.Utilities.SpaceTime;
using Google.Protobuf.WellKnownTypes;

namespace Bearded.TD.Game.Buildings
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
        public override void Focus(SelectionManager selectionManager)
            => throw new InvalidOperationException(selectionIsImmutable);
        public override void Select(SelectionManager selectionManager)
            => throw new InvalidOperationException(selectionIsImmutable);

        protected override IEnumerable<IComponent<BuildingGhost>> InitialiseComponents()
            => Blueprint.GetComponentsForGhost();

        public override void Draw(GeometryManager geometries)
        {
            var anyTileOutsideWorkerNetwork = false;
            
            var workerNetwork = Faction.WorkerNetwork;
            foreach (var tile in Footprint.OccupiedTiles)
            {
                var baseColor = Color.Green;

                var tileIsOutsideWorkerNetwork = Game.Level.IsValid(tile) && !workerNetwork.IsInRange(tile);
                anyTileOutsideWorkerNetwork |= tileIsOutsideWorkerNetwork;
                
                if (!Game.BuildingPlacementLayer.IsTileValidForBuilding(tile))
                {
                    baseColor = Color.Red;
                }
                else if (tileIsOutsideWorkerNetwork)
                {
                    baseColor = Color.Orange;
                }

                var color = baseColor * 0.5f;
                DrawTile(geometries, color, tile);
            }

            if (anyTileOutsideWorkerNetwork)
            {
                renderWorkerNetworkBorderCloseBy(geometries, new Unit(10), Color.OrangeRed);
            }
            else
            {
                renderWorkerNetworkBorderCloseBy(geometries, new Unit(5), Color.DodgerBlue);
            }
            
            base.Draw(geometries);
        }

        private void renderWorkerNetworkBorderCloseBy(GeometryManager geometries, Unit maxDistance, Color baseColor)
        {
            var maxDistanceSquared = maxDistance.Squared;
            
            var workerNetwork = Faction.WorkerNetwork;
            var networkBorder = TileAreaBorder.From(Game.Level, workerNetwork.IsInRange);
            
            TileAreaBorderRenderer.Render(networkBorder, geometries.ConsoleBackground, getLineColor);

            Color? getLineColor(Position2 point1, Position2 point2)
            {
                var lineCenter = point1 + (point2 - point1) * 0.5f;
                var distanceToLineSquared = (lineCenter - Position).LengthSquared;

                var alpha = 1 - distanceToLineSquared / maxDistanceSquared;

                return alpha < 0 ? (Color?)null : baseColor * alpha;
            }
        }
    }
}
