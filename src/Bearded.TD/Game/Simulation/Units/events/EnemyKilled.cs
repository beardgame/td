using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Units;

readonly record struct EnemyKilled(GameObject Unit) : IGlobalEvent;
