namespace Bearded.TD.Game.Simulation.GameObjects;

readonly record struct ComponentAdded(IComponent Component) : IComponentEvent;