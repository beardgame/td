using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;

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

        protected override IEnumerable<IComponent<BuildingGhost>> InitialiseComponents()
            => Blueprint.GetComponentsForGhost();

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;

            foreach (var tile in Footprint.OccupiedTiles)
            {
                geo.Color = (tile.IsValid && tile.Info.IsPassable && tile.Info.PlacedBuilding == null ? Color.Green : Color.Red) * 0.5f;
                geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
            }
            
            base.Draw(geometries);
        }
    }
}
