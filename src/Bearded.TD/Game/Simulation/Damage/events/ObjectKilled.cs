using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

[Trigger("objectKilled")]
readonly record struct ObjectKilled(IDamageSource? LastDamageSource) : IComponentEvent;
