using System;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface ITrigger
{
    ITriggerSubscription Subscribe(ComponentEvents events, Action action);
}
