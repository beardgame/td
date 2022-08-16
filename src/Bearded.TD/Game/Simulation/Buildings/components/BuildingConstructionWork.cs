using System.Linq;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingConstructionWork : BuildingWork
{
    public new ResourceAmount? ResourcesInvestedSoFar => base.ResourcesInvestedSoFar;

    protected override ResourceAmount Cost =>
        Owner.GetComponents<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
    protected override ResourceRate ConsumptionRate => Constants.Game.Resources.ConstructionSpeed;

    public BuildingConstructionWork(IIncompleteBuilding incompleteBuilding) : base(incompleteBuilding) { }
}
