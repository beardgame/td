using Bearded.TD.Game.Simulation.Buildings;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Footprints;

interface IObjectFootprint
{
    Footprint Footprint { get; }
}

[Component("footprint")]
sealed class ObjectFootprint : Component<ObjectFootprint.IParameters>, IObjectFootprint, IBuildBuildingPrecondition
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        public Footprint Group { get; }
    }

    Footprint IObjectFootprint.Footprint => Parameters.Group;

    public ObjectFootprint(IParameters parameters) : base(parameters)
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
