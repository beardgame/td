using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints
{
    [Component("footprint")]
    sealed class FootprintGroup<T> : Component<T, Content.Models.IFootprintGroup>, IFootprintGroup
    {
        FootprintGroup IFootprintGroup.FootprintGroup => Parameters.Group;

        public FootprintGroup(Content.Models.IFootprintGroup parameters) : base(parameters) { }

        protected override void Initialize() { }
        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(CoreDrawers drawers) { }
    }

    interface IFootprintGroup
    {
        FootprintGroup FootprintGroup { get; }
    }
}
