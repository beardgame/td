using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("testDependency")]
sealed class TestDependency : Component, ITestDependency
{
    protected override void OnAdded() { }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime) { }

    public void DoTheThing()
    {
        Owner.Game.Meta.Logger.Info?.Log("Method on dependency called!");
    }
}

interface ITestDependency
{
    void DoTheThing();
}
