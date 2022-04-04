using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class GameObject
    : IComponentOwner<GameObject>, IDeletable, IGameObject, IPositionable, IDirected
{
    private GameState? game;
    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    // Separate nullable field for lazy initialisation in Add.
    public GameState Game => game!;

    public IComponentOwner? Parent { get; }
    public Position3 Position { get; set; }
    public Direction2 Direction { get; set; }

    private readonly ComponentCollection components;
    private readonly ComponentEvents events = new();

    public bool Deleted { get; private set; }
    public event VoidEventHandler? Deleting;

    public GameObject(IComponentOwner? parent, Position3 position, Direction2 direction)
    {
        Direction = direction;
        Parent = parent;
        Position = position;
        components = new ComponentCollection(this, events);
    }

    public void Add(GameState gameState)
    {
        if (gameState.ObjectBeingAdded != this || game != null)
        {
            throw new Exception("Tried adding game object to game in unexpected circumstances.");
        }

        game = gameState;
    }

    public void Update(TimeSpan elapsedTime)
    {
        components.Update(elapsedTime);
    }

    public void Delete()
    {
        events.Send(new ObjectDeleting());
        Deleting?.Invoke();
        Deleted = true;
    }

    public bool CanApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.CanApplyTo(this);

    public void ApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.ApplyTo(this);

    public bool RemoveUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.RemoveFrom(this);


    public void AddComponent(IComponent component) => components.Add(component);

    public void RemoveComponent(IComponent component) => components.Remove(component);

    public IEnumerable<TComponent> GetComponents<TComponent>() => components.Get<TComponent>();

    IEnumerable<TComponent> IComponentOwner<GameObject>.GetComponents<TComponent>() =>
        components.Get<TComponent>();

    IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
}