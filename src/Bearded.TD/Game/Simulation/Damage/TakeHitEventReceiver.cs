using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("takeHitEventReceiver")]
sealed class TakeHitEventReceiver : EventReceiver<TakeHit> {}
