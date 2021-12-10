using System.Collections.Generic;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components;

[ComponentOwner]
sealed class ComponentGameObject : GameObject, IComponentOwner<ComponentGameObject>, IPositionable, IDirected
{
    public IComponentOwner? Parent { get; }
    public Position3 Position { get; set; }
    public Direction2 Direction { get; private set; }

    private readonly ComponentCollection<ComponentGameObject> components;
    private readonly ComponentEvents events = new();

    public ComponentGameObject(IComponentOwner? parent, Position3 position, Direction2 direction)
    {
        Direction = direction;
        Parent = parent;
        Position = position;
        components = new ComponentCollection<ComponentGameObject>(this, events);
    }

    protected override void OnDelete()
    {
        events.Send(new ObjectDeleting());
        base.OnDelete();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        components.Update(elapsedTime);
    }

    public override void Draw(CoreDrawers drawers)
    {
        components.Draw(drawers);
    }

    public void AddComponent(IComponent<ComponentGameObject> component) => components.Add(component);

    public void RemoveComponent(IComponent<ComponentGameObject> component) => components.Remove(component);

    public IEnumerable<TComponent> GetComponents<TComponent>() => components.Get<TComponent>();

    IEnumerable<TComponent> IComponentOwner<ComponentGameObject>.GetComponents<TComponent>() =>
        components.Get<TComponent>();

    IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
}