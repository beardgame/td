using System;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("canBeBuiltOn")]
class CanBeBuiltOn : Component
{
    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void Replace()
    {
        tryRefund();
        Owner.Delete();
    }

    private void tryRefund()
    {
        var state = Owner.GetComponents<IBuildingStateProvider>().Single().State;
        if (!state.IsMaterialized)
            return;

        if (Owner.GetComponents<BuildingConstructionWork>().SingleOrDefault() is { } constructionWork)
        {
            refund(constructionWork.ResourcesInvestedSoFar ?? ResourceAmount.Zero);
        }
        else if(Owner.GetComponents<ICost>().SingleOrDefault() is { } cost)
        {
            refund(cost.Resources);
        }
    }

    private void refund(ResourceAmount value)
    {
        var faction = Owner.FindFaction();
        if (!faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            throw new InvalidOperationException();
        resources.ProvideResources(value);
    }
}
