using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Content.Serialization.Converters;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class BlueprintLoadingContext
{
    public ModLoadingContext Context { get; }
    public ModMetadata Meta { get; }
    public JsonSerializer Serializer { get; }
    public ReadOnlyCollection<Mod> LoadedDependencies { get; }

    private readonly Dictionary<Type, object> dependencyResolvers = new Dictionary<Type, object>();

    public BlueprintLoadingContext(ModLoadingContext context, ModMetadata meta, JsonSerializer serializer,
        ReadOnlyCollection<Mod> loadedDependencies)
    {
        Context = context;
        Meta = meta;
        Serializer = serializer;
        LoadedDependencies = loadedDependencies;
    }

    public void AddDependencyResolver<T>(IDependencyResolver<T> dependencyResolver)
    {
        dependencyResolvers[typeof(T)] = dependencyResolver;
        Serializer.Converters.Add(new DependencyConverter<T>(dependencyResolver));
    }

    public IDependencyResolver<T> FindDependencyResolver<T>() =>
        dependencyResolvers[typeof(T)] as IDependencyResolver<T>;
}