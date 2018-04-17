using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Model;

namespace Bearded.TD.Game.Buildings
{
    [ComponentOwner]
    class BuildingGhost : BuildingBase<BuildingGhost>
    {
        public BuildingGhost(BuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
        }

        public void SetFootprint(PositionedFootprint footprint) => ChangeFootprint(footprint);

        public override SelectionState SelectionState => SelectionState.Selected;

        public override void ResetSelection() => throw new InvalidOperationException();
        public override void Focus(SelectionManager selectionManager) => throw new InvalidOperationException();
        public override void Select(SelectionManager selectionManager) => throw new InvalidOperationException();

        protected override IEnumerable<IComponent<BuildingGhost>> InitialiseComponents()
            => Blueprint.GetComponentsForGhost();

        public override void Draw(GeometryManager geometries)
        {
            foreach (var tile in Footprint.OccupiedTiles)
            {
                var color = (tile.IsValid && tile.Info.IsPassableFor(TileInfo.PassabilityLayer.Building) ? Color.Green : Color.Red) * 0.5f;
                DrawTile(geometries, color, tile);
            }
            
            base.Draw(geometries);
        }
    }
}
