using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("canBeBuiltOn")]
class CanBeBuiltOn : Component
{
    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void Replace()
    {
        Owner.TryRefund();
        Owner.Delete();
    }
}
