using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("inheritDirection")]
sealed class InheritDirection : Component, IDirected3
{
    public Difference3 Direction { get; private set; }

    protected override void OnAdded()
    {
        if (Owner.Parent is { } p && p.TryGetSingleComponent<IDirected3>(out var directed))
        {
            Direction = directed.Direction;
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
