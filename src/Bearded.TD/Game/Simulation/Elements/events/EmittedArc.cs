using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements.events;

[Trigger("emittedArc")]
readonly record struct EmittedArc : IComponentEvent;
