using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints;

[Component("footprint")]
sealed class FootprintGroup : Component<Content.Models.IFootprintGroup>, IFootprintGroup
{
    World.FootprintGroup IFootprintGroup.FootprintGroup => Parameters.Group;

    public FootprintGroup(Content.Models.IFootprintGroup parameters) : base(parameters) { }

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }
}

interface IFootprintGroup
{
    World.FootprintGroup FootprintGroup { get; }
}
