namespace Bearded.TD.Game.Simulation.Components
{
    readonly struct ComponentRemoved : IComponentEvent
    {
        public IComponent Component { get; }

        public ComponentRemoved(IComponent component)
        {
            Component = component;
        }
    }
}
