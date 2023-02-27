using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Linq;
using IComponent = Bearded.TD.Game.Simulation.GameObjects.IComponent;
using IComponentModel = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Content.Models;

sealed class GameObjectBlueprintProxy : IGameObjectBlueprint
{
    private GameObjectBlueprint? blueprint;
    public ModAwareId Id { get; }

    public GameObjectBlueprintProxy(ModAwareId id)
    {
        Id = id;
    }

    public void InjectActualBlueprint(GameObjectBlueprint actualBlueprint)
    {
        if (blueprint != null)
            throw new InvalidOperationException("Cannot inject blueprint more than once.");
        if (actualBlueprint.Id != Id)
            throw new InvalidOperationException("Id of injected blueprint must match.");

        blueprint = actualBlueprint;
    }

    IEnumerable<IComponent> IGameObjectBlueprint.GetComponents() => blueprint!.GetComponents();
    IEnumerable<IComponentFactory> IGameObjectBlueprint.GetFactories() => blueprint!.GetFactories();
}

sealed class GameObjectBlueprint : IGameObjectBlueprint
{
    public ModAwareId Id { get; }
    private readonly ImmutableArray<IComponentModel> componentParameters;

    private IReadOnlyCollection<IComponentFactory>? factories;

    public IEnumerable<IComponent> GetComponents()
    {
        return GetFactories().Select(f => f.Create());
    }

    public IEnumerable<IComponentFactory> GetFactories()
    {
        return factories ??= createFactories();
    }

    private IReadOnlyCollection<IComponentFactory> createFactories()
    {
        factories = componentParameters.Select(ComponentFactories.CreateComponentFactory).NotNull()
            .ToList().AsReadOnly();

        return factories;
    }

    public GameObjectBlueprint(ModAwareId id, IEnumerable<IComponentModel> components)
    {
        Id = id;
        componentParameters = components.ToImmutableArray();
    }
}
