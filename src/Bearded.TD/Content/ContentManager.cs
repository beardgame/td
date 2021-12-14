using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.Content;

sealed class ContentManager
{
    private static readonly ModSorter sorter = new ModSorter();

    private readonly ModLoadingContext loadingContext;
    public ModLoadingProfiler LoadingProfiler => loadingContext.Profiler;

    private readonly ImmutableDictionary<string, ModMetadata> modsById;
    public ImmutableHashSet<ModMetadata> AvailableMods { get; }

    private readonly HashSet<ModMetadata> enabledMods = new HashSet<ModMetadata>();
    public ImmutableHashSet<ModMetadata> EnabledMods => enabledMods.ToImmutableHashSet();

    private readonly Dictionary<ModMetadata, ModForLoading> modsForLoading =
        new Dictionary<ModMetadata, ModForLoading>();
    private readonly Queue<ModMetadata> modLoadingQueue = new Queue<ModMetadata>();

    public Maybe<ModMetadata> CurrentlyLoading { get; private set; } = Maybe.Nothing;
    public bool IsFinishedLoading => CurrentlyLoading.Select(_ => false).ValueOrDefault(modLoadingQueue.Count == 0);

    public IEnumerable<Mod> LoadedEnabledMods
    {
        get
        {
            if (!IsFinishedLoading)
            {
                throw new InvalidOperationException("Cannot access loaded enabled mods before finished loading.");
            }

            return enabledMods.Select(metadata => modsForLoading[metadata].GetLoadedMod()).ToImmutableArray();
        }
    }

    public ContentManager(
        Logger logger, IGraphicsLoader graphicsLoader, IReadOnlyCollection<ModMetadata> availableMods)
    {
        loadingContext = new ModLoadingContext(logger, graphicsLoader, new ModLoadingProfiler());

        AvailableMods = ImmutableHashSet.CreateRange(availableMods);
        modsById = availableMods.ToImmutableDictionary(m => m.Id);
    }

    public ModMetadata FindMod(string modId) => modsById[modId];

    public void SetEnabledMods(IEnumerable<ModMetadata> mods)
    {
        enabledMods.Clear();
        sorter.SortByDependency(mods).ForEach(EnableMod);
    }

    public void EnableMod(ModMetadata mod)
    {
        if (enabledMods.Contains(mod))
        {
            return;
        }

        // Make sure all dependencies are enabled too.
        foreach (var dependency in mod.Dependencies)
        {
            var dependencyMod = modsById[dependency.Id];
            if (!enabledMods.Contains(dependencyMod))
            {
                EnableMod(dependencyMod);
            }
        }

        // Enqueue for loading if it hasn't been loaded yet.
        if (!modsForLoading.ContainsKey(mod))
        {
            modsForLoading.Add(mod, new ModForLoading(mod));
            modLoadingQueue.Enqueue(mod);
        }

        enabledMods.Add(mod);
    }

    public void DisableMod(ModMetadata mod)
    {
        if (!enabledMods.Remove(mod))
        {
            return;
        }

        var dependents = enabledMods.Where(m => m.Dependencies.Any(d => d.Id == mod.Id)).ToList();
        dependents.ForEach(DisableMod);
    }

    public void Update(UpdateEventArgs args)
    {
        pumpLoadingQueue();
    }

    private void pumpLoadingQueue()
    {
        CurrentlyLoading.Match(metadata =>
        {
            var modForLoading = modsForLoading[metadata];
            if (!modForLoading.IsDone)
            {
                return;
            }
            // TODO: deal with errors
            CurrentlyLoading = Maybe.Nothing;
            startLoadingNextMod();
        }, startLoadingNextMod);
    }

    private void startLoadingNextMod()
    {
        ModMetadata metadata;
        do
        {
            metadata = modLoadingQueue.Count == 0 ? null : modLoadingQueue.Dequeue();
        } while (metadata != null && !enabledMods.Contains(metadata));

        if (metadata == null)
        {
            return;
        }

        var modForLoading = modsForLoading[metadata];
        var loadedDependencies = metadata.Dependencies
            .Select(dependency => modsById[dependency.Id])
            .Select(dependencyMeta => modsForLoading[dependencyMeta])
            .Select(dependencyModForLoading => dependencyModForLoading.GetLoadedMod())
            .ToList()
            .AsReadOnly();

        modForLoading.StartLoading(loadingContext, loadedDependencies);
        CurrentlyLoading = Maybe.Just(metadata);
    }

    public void CleanUp()
    {
        DebugAssert.State.Satisfies(IsFinishedLoading);
        var unusedMods = modsForLoading.Where(m => !enabledMods.Contains(m.Key)).ToList();
        foreach (var (metadata, modForLoading) in unusedMods)
        {
            GraphicsUnloader.CleanUp(modForLoading.GetLoadedMod().Blueprints);
            modsForLoading.Remove(metadata);
        }
    }

    public void CleanUpAll()
    {
        DebugAssert.State.Satisfies(IsFinishedLoading);
        foreach (var (_, modForLoading) in modsForLoading)
        {
            GraphicsUnloader.CleanUp(modForLoading.GetLoadedMod().Blueprints);
        }
        enabledMods.Clear();
        modsForLoading.Clear();
    }
}