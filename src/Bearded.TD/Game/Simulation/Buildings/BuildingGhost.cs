using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class BuildingGhost : BuildingBase<BuildingGhost>
    {
        public override IBuildingState State { get; } = new GhostBuildingState();

        public BuildingGhost(IBuildingBlueprint blueprint)
            : base(blueprint) {}

        protected override IEnumerable<IComponent<BuildingGhost>> InitializeComponents() =>
            Blueprint.GetComponentsForGhost();
    }
}
