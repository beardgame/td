using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

[Trigger("objectRepaired")]
readonly record struct ObjectRepaired : IComponentEvent;
