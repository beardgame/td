using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.GameObjects;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class GameObjectBlueprintLoader : BaseBlueprintLoader<IGameObjectBlueprint, Serialization.Models.GameObjectBlueprint, Void>
{
    protected override string RelativePath => "defs/blueprints";
    protected override DependencySelector SelectDependency => m => m.Blueprints.GameObjects;

    private readonly GameObjectProxyBlueprintResolver gameObjectProxyBlueprintCollector;
    private readonly DependencyConverter<IGameObjectBlueprint> dependencyConverter;
    private readonly Dictionary<ModAwareId, (GameObjectBlueprint Blueprint, FileInfo File, List<GameObjectBlueprintProxy> Dependencies)>
        dependenciesByBlueprintId = new();

    public GameObjectBlueprintLoader(BlueprintLoadingContext context) : base(context)
    {
        gameObjectProxyBlueprintCollector = new GameObjectProxyBlueprintResolver(Context.Meta, Context.LoadedDependencies);
        dependencyConverter = new DependencyConverter<IGameObjectBlueprint>(gameObjectProxyBlueprintCollector);
    }

    public override ReadonlyBlueprintCollection<IGameObjectBlueprint> LoadBlueprints()
    {
        var blueprints = loadBlueprints();

        var validBlueprints = removeMissingDependencies(blueprints);

        validBlueprints.ForEach(injectDependencies);

        var blueprintCollection = new ReadonlyBlueprintCollection<IGameObjectBlueprint>(validBlueprints);

        SetupDependencyResolver(blueprintCollection);

        return blueprintCollection;
    }

    private List<IGameObjectBlueprint> loadBlueprints()
    {
        Context.Serializer.Converters.Add(dependencyConverter);

        var files = GetJsonFiles();

        var blueprints = LoadBlueprintsFromFiles(files);

        Context.Serializer.Converters.Remove(dependencyConverter);

        return blueprints;
    }

    private List<IGameObjectBlueprint> removeMissingDependencies(List<IGameObjectBlueprint> blueprints)
    {
        var blueprintCandidates = blueprints.ToList();

        var thereMayBeMoreMissingDependencies = true;

        while (thereMayBeMoreMissingDependencies)
        {
            thereMayBeMoreMissingDependencies = false;

            blueprintCandidates = blueprintCandidates.Where(allDependenciesAreValidCandidates).ToList();
        }

        return blueprintCandidates;

        bool allDependenciesAreValidCandidates(IGameObjectBlueprint candidate)
        {
            var (_, file, proxies) = dependenciesByBlueprintId[candidate.Id];

            foreach (var proxy in proxies)
            {
                if (dependenciesByBlueprintId.ContainsKey(proxy.Id))
                    continue;

                LogError($"Error loading '{Context.Meta.Id}/{RelativePath}/../{file.Name}': Could not find reference '{proxy.Id}'");
                dependenciesByBlueprintId.Remove(candidate.Id);
                thereMayBeMoreMissingDependencies = true;

                return false;
            }

            return true;
        }
    }

    private void injectDependencies(IGameObjectBlueprint blueprint)
    {
        var (_, _, proxies) = dependenciesByBlueprintId[blueprint.Id];

        foreach (var proxy in proxies)
        {
            if (!dependenciesByBlueprintId.TryGetValue(proxy.Id, out var dependency))
            {
                throw new InvalidOperationException("Invalid dependencies should have been removed before.");
            }

            proxy.InjectActualBlueprint(dependency.Blueprint);
        }
    }

    protected override IGameObjectBlueprint LoadBlueprint(FileInfo file)
    {
        var blueprint = base.LoadBlueprint(file);

        var proxies = gameObjectProxyBlueprintCollector.GetAndResetCurrentProxies();

        dependenciesByBlueprintId[blueprint.Id] = ((GameObjectBlueprint) blueprint, file, proxies);

        return blueprint;
    }
}
