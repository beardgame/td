using System.Linq;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingConstructionWork : BuildingWork
{
    public new ResourceAmount? ResourcesInvestedSoFar => base.ResourcesInvestedSoFar;

    protected override ResourceAmount Cost =>
        Owner.GetComponents<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;

    public BuildingConstructionWork(IIncompleteBuilding incompleteBuilding) : base(incompleteBuilding) { }
}
