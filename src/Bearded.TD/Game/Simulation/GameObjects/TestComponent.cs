using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("testComponent")]
sealed partial class TestComponent : Component
{
    [Inject] private ITestDependency myDependency = null!;

    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate(); // important!
        myDependency.DoTheThing();
    }

    public override void Update(TimeSpan elapsedTime) { }
}
