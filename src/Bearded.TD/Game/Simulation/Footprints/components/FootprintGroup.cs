using Bearded.TD.Game.Simulation.Buildings;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Footprints;

interface IFootprintGroup
{
    World.FootprintGroup FootprintGroup { get; }
}

[Component("footprint")]
sealed class FootprintGroup : Component<Content.Models.IFootprintGroup>, IFootprintGroup, IBuildBuildingPrecondition
{
    World.FootprintGroup IFootprintGroup.FootprintGroup => Parameters.Group;

    public FootprintGroup(Content.Models.IFootprintGroup parameters) : base(parameters)
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
