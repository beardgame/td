using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;

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
            foreach (var tile in Footprint.OccupiedTiles)
            {
                var baseColor = Game.BuildingPlacementLayer.IsTileValidForBuilding(tile)
                    ? Color.Green
                    : Color.Red;
                var color = baseColor * 0.5f;
                DrawTile(geometries, color, tile);
            }
            
            base.Draw(geometries);
        }
    }
}
