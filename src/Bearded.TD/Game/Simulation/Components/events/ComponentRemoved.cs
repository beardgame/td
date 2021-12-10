namespace Bearded.TD.Game.Simulation.Components
{
    readonly record struct ComponentRemoved(IComponent Component) : IComponentEvent;
}
