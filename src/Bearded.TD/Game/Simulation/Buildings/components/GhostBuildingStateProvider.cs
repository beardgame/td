using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class GhostBuildingStateProvider<T> : Component<T>, IBuildingStateProvider
    {
        public IBuildingState State { get; } = new GhostBuildingState();

        protected override void Initialize() { }
        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(CoreDrawers drawers) { }
    }
}
