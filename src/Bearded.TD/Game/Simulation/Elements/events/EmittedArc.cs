using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements;

[Trigger("emittedArc")]
readonly record struct EmittedArc : IComponentEvent;
