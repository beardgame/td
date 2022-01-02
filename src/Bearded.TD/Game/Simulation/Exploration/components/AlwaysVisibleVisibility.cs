using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

sealed class AlwaysVisibleVisibility<T> : Component<T>, IVisibility
{
    public ObjectVisibility Visibility => ObjectVisibility.Visible;

    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}
}