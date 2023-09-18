using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

[Trigger("objectRuined")]
readonly record struct ObjectRuined : IComponentEvent;
