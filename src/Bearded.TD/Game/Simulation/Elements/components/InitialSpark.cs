using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("initialSpark")]
class InitialSpark : Component
{
    private bool sparked;

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (sparked)
            return;

        Events.Send(new Spark());
        sparked = true;
    }
}
