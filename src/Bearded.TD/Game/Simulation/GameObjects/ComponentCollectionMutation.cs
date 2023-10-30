namespace Bearded.TD.Game.Simulation.GameObjects;

readonly struct ComponentCollectionMutation
{
    public static ComponentCollectionMutation Addition(IComponent component) =>
        new(CollectionMutationType.Addition, component);

    public static ComponentCollectionMutation Removal(IComponent component) =>
        new(CollectionMutationType.Removal, component);

    public CollectionMutationType Type { get; }
    public IComponent Component { get; }

    private ComponentCollectionMutation(CollectionMutationType type, IComponent component)
    {
        Type = type;
        Component = component;
    }

    public ComponentCollectionMutation Reversed() => new(
        Type == CollectionMutationType.Addition ? CollectionMutationType.Removal : CollectionMutationType.Addition,
        Component);
}

enum CollectionMutationType
{
    Addition,
    Removal
}
