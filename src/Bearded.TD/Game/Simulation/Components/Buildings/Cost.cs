using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Buildings
{
    [Component("cost")]
    sealed class Cost<T> : Component<T, Content.Models.ICost>, ICost
    {
        public ResourceAmount Resources => Parameters.Resources;

        public Cost(Content.Models.ICost parameters) : base(parameters) { }

        protected override void Initialize() { }
        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(CoreDrawers drawers) { }
    }
}
