using Bearded.TD.Game.Simulation.Buildings;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Footprints;

interface IFootprintGroup
{
    FootprintGroup FootprintGroup { get; }
}

[Component("footprint")]
sealed class Footprint : Component<Footprint.IParameters>, IFootprintGroup, IBuildBuildingPrecondition
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        public FootprintGroup Group { get; }
    }

    FootprintGroup IFootprintGroup.FootprintGroup => Parameters.Group;

    public Footprint(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public Result CanBuild(Parameters parameters)
    {
        return Result.FromBool(Parameters.Group == parameters.Footprint.Footprint);
    }
}
