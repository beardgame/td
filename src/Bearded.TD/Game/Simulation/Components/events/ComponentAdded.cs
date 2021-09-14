namespace Bearded.TD.Game.Simulation.Components
{
    readonly struct ComponentAdded : IComponentEvent
    {
        public IComponent Component { get; }

        public ComponentAdded(IComponent component)
        {
            Component = component;
        }
    }
}
