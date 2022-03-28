using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.Linq;
using IComponent = Bearded.TD.Game.Simulation.GameObjects.IComponent;
using IComponentModel = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Content.Models;

sealed class ComponentOwnerBlueprintProxy : IComponentOwnerBlueprint
{
    private ComponentOwnerBlueprint? blueprint;
    public ModAwareId Id { get; }

    public ComponentOwnerBlueprintProxy(ModAwareId id)
    {
        Id = id;
    }

    public void InjectActualBlueprint(ComponentOwnerBlueprint actualBlueprint)
    {
        if (blueprint != null)
            throw new InvalidOperationException("Cannot inject blueprint more than once.");
        if (actualBlueprint.Id != Id)
            throw new InvalidOperationException("Id of injected blueprint must match.");

        blueprint = actualBlueprint;
    }

    IEnumerable<IComponent> IComponentOwnerBlueprint.GetComponents() => blueprint!.GetComponents();
    bool IComponentOwnerBlueprint.CanApplyUpgradeEffect(IUpgradeEffect effect) => blueprint!.CanApplyUpgradeEffect(effect);
}

sealed class ComponentOwnerBlueprint : IComponentOwnerBlueprint
{
    public ModAwareId Id { get; }
    private readonly ImmutableArray<IComponentModel> componentParameters;

    private IReadOnlyCollection<IComponentFactory>? factories;

    public IEnumerable<IComponent> GetComponents()
    {
        return getFactories().Select(f => f.Create());
    }

    private IEnumerable<IComponentFactory> getFactories()
    {
        return factories ??= createFactories();
    }

    private IReadOnlyCollection<IComponentFactory> createFactories()
    {
        factories = componentParameters.Select(ComponentFactories.CreateComponentFactory).NotNull()
            .ToList().AsReadOnly();

        return factories;
    }

    public ComponentOwnerBlueprint(ModAwareId id, IEnumerable<IComponentModel> components)
    {
        Id = id;
        componentParameters = components.ToImmutableArray();
    }

    public bool CanApplyUpgradeEffect(IUpgradeEffect effect)
    {
        return getFactories().Any(f => f.CanApplyUpgradeEffect(effect));
    }
}
