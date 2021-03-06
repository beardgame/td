﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.Linq;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Content.Models
{
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

        IEnumerable<IComponent<T>> IComponentOwnerBlueprint.GetComponents<T>() => blueprint!.GetComponents<T>();
        bool IComponentOwnerBlueprint.CanApplyUpgradeEffect<T>(IUpgradeEffect effect) => blueprint!.CanApplyUpgradeEffect<T>(effect);
    }

    sealed class ComponentOwnerBlueprint : IComponentOwnerBlueprint
    {
        public ModAwareId Id { get; }
        private readonly IReadOnlyCollection<IComponent> componentParameters;

        private readonly Dictionary<Type, object> componentFactoriesByOwnerType = new Dictionary<Type, object>();

        public IEnumerable<IComponent<T>> GetComponents<T>()
        {
            return getFactories<T>().Select(f => f.Create());
        }

        private IReadOnlyCollection<IComponentFactory<T>> getFactories<T>()
        {
            if (componentFactoriesByOwnerType.TryGetValue(typeof(T), out var factories))
                return (IReadOnlyCollection<IComponentFactory<T>>) factories;

            return createFactories<T>();
        }

        private IReadOnlyCollection<IComponentFactory<T>> createFactories<T>()
        {
            var factories = componentParameters.Select(ComponentFactories.CreateComponentFactory<T>).NotNull()
                .ToList().AsReadOnly();

            componentFactoriesByOwnerType.Add(typeof(T), factories);

            return factories;
        }

        public ComponentOwnerBlueprint(ModAwareId id, IEnumerable<IComponent>? components)
        {
            Id = id;

            componentParameters = (components?.ToList() ?? new List<IComponent>()).AsReadOnly();
        }

        public bool CanApplyUpgradeEffect<T>(IUpgradeEffect effect)
        {
            return getFactories<T>().Any(f => f.CanApplyUpgradeEffect(effect));
        }
    }
}
