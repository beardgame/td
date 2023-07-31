using System;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class ThrowOnActivate : Component
{
    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}

    public override void Activate()
    {
        throw new InvalidOperationException(
            "A game object with the 'throw on activate' component can never be added to the game");
    }
}
