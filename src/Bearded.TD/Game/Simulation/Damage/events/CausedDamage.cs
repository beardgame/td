using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct CausedDamage(FinalDamageResult Result, GameObject Target) : IComponentEvent;
