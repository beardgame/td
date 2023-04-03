using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("attachToParent")]
sealed class AttachToParent : Component
{
    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Owner.Parent is not { } parent)
            return;

        Owner.Position = parent.Position;
        Owner.Direction = parent.Direction;
    }
}

