using System.Linq;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingConstructionWork : BuildingWork
{
    private readonly ResourceAmount additionalCost;

    private ResourceAmount baseCost => Owner.GetComponents<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;

    public new ResourceAmount? ResourcesInvestedSoFar => base.ResourcesInvestedSoFar;

    protected override ResourceAmount Cost => baseCost + additionalCost;

    protected override ResourceRate ConsumptionRate => Constants.Game.Resources.ConstructionSpeed;

    public BuildingConstructionWork(IIncompleteBuilding incompleteBuilding, ResourceAmount additionalCost) : base(incompleteBuilding)
    {
        this.additionalCost = additionalCost;
    }
}
