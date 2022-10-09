using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Weapons;

readonly record struct EnemyMarked(GameObject GameObject, UntypedDamage Damage) : IComponentEvent;
