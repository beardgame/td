namespace Bearded.TD.Game.Simulation.GameObjects;

readonly record struct ComponentRemoved(IComponent Component) : IComponentEvent;