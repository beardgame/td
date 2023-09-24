namespace Bearded.TD.Game.Simulation.GameObjects;

[Trigger("objectDeleting")]
readonly record struct ObjectDeleting : IComponentEvent;
