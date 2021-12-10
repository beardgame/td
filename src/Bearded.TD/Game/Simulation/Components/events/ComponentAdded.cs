namespace Bearded.TD.Game.Simulation.Components
{
    readonly record struct ComponentAdded(IComponent Component) : IComponentEvent;
}
