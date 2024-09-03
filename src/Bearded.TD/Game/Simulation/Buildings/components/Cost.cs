using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;

namespace Bearded.TD.Game.Simulation.Buildings;

interface ICost
{
    Resource<Scrap> Resources { get; }
}

[Component("cost")]
sealed class Cost : Component<Cost.IParameters>, ICost, IBuildBuildingPrecondition
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        public Resource<Scrap> Resources { get; }
    }

    public Resource<Scrap> Resources => Parameters.Resources;

    public Cost(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }

    public Result CanBuild(Parameters parameters) => Result.FromCost(Parameters.Resources);
}
