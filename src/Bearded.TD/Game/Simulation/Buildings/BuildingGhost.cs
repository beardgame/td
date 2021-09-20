using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class BuildingGhost : BuildingBase<BuildingGhost>
    {
        public BuildingGhost(IBuildingBlueprint blueprint)
            : base(blueprint) {}

        protected override IEnumerable<IComponent<BuildingGhost>> InitializeComponents() =>
            Blueprint.GetComponentsForGhost();
    }
}
