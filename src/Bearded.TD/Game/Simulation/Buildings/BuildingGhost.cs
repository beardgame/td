using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class BuildingGhost : BuildingBase<BuildingGhost>
    {
        public override IBuildingState State { get; } = new GhostBuildingState();

        public BuildingGhost(IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint) {}

        public void SetFootprint(PositionedFootprint footprint) => ChangeFootprint(footprint);

        protected override IEnumerable<IComponent<BuildingGhost>> InitializeComponents()
            => new BuildingGhostDrawing().Yield().Concat(Blueprint.GetComponentsForGhost());
    }
}
