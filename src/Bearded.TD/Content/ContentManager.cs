using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;

namespace Bearded.TD.Content;

sealed class ContentManager
{
    private readonly ModLoadingContext loadingContext;
    private readonly ImmutableArray<ModMetadata> allMods;
    private readonly ImmutableDictionary<string, ModMetadata> modsById;

    private readonly Dictionary<ModMetadata, ModForLoading> modsForLoading = new();
    private readonly Queue<ModMetadata> modLoadingQueue = new();
    private readonly MultiDictionary<ModMetadata, ModLease> leasesByMods = new();

    private ModMetadata? currentlyLoading;
    private bool isFinishedLoading => currentlyLoading is null && modLoadingQueue.Count == 0;

    public ModLoadingProfiler LoadingProfiler => loadingContext.Profiler;

    public ImmutableArray<ModMetadata> VisibleMods => allMods.Where(m => m.Visible).ToImmutableArray();

    public ContentManager(Logger logger, IGraphicsLoader graphicsLoader, IReadOnlyCollection<ModMetadata> allMods)
    {
        loadingContext = new ModLoadingContext(logger, graphicsLoader, new ModLoadingProfiler());

        this.allMods = ImmutableArray.CreateRange(allMods);
        modsById = allMods.ToImmutableDictionary(m => m.Id);
    }

    public ModMetadata FindMod(string modId) => modsById[modId];


    public void Update()
    {
        pumpLoadingQueue();
    }

    private void pumpLoadingQueue()
    {
        if (currentlyLoading is { } metadata)
        {
            var modForLoading = modsForLoading[metadata];
            if (!modForLoading.IsDone)
            {
                return;
            }
            // TODO: deal with errors
            currentlyLoading = null;
        }

        if (currentlyLoading is null)
        {
            startLoadingNextMod();
        }
    }

    private void startLoadingNextMod()
    {
        ModMetadata? metadata;
        do
        {
            metadata = modLoadingQueue.Count == 0 ? null : modLoadingQueue.Dequeue();
        } while (metadata != null && (!modsForLoading.ContainsKey(metadata) || !leasesByMods.ContainsKey(metadata)));

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
        currentlyLoading = metadata;
    }

    public void CleanUpUnused()
    {
        var unusedMods = modsForLoading.Where(kvp => !leasesByMods.ContainsKey(kvp.Key)).ToImmutableArray();

        foreach (var (metadata, modForLoading) in unusedMods)
        {
            // We have no way to abort loading. Just finish loading it and we'll pick it up in a future clean-up cycle.
            if (currentlyLoading == metadata)
            {
                continue;
            }

            if (modForLoading.IsDone)
            {
                GraphicsUnloader.CleanUp(modForLoading.GetLoadedMod().Blueprints);
            }
            modsForLoading.Remove(metadata);
        }
    }

    public void CleanUpAll()
    {
        DebugAssert.State.Satisfies(isFinishedLoading);
        foreach (var (_, modForLoading) in modsForLoading)
        {
            GraphicsUnloader.CleanUp(modForLoading.GetLoadedMod().Blueprints);
        }
        leasesByMods.Clear();
        modsForLoading.Clear();
    }

    public IModLease LeaseMod(ModMetadata mod)
    {
        var lease = new ModLease(this, mod, findModForLoading(mod));
        leasesByMods.Add(mod, lease);
        return lease;
    }

    private ModForLoading findModForLoading(ModMetadata metadata)
    {
        if (modsForLoading.TryGetValue(metadata, out var mod))
        {
            return mod;
        }

        var m = new ModForLoading(metadata);
        modsForLoading.Add(metadata, m);
        modLoadingQueue.Enqueue(metadata);
        return m;
    }

    private void release(ModLease lease)
    {
        leasesByMods.Remove(lease.Metadata, lease);
    }

    private sealed class ModLease(ContentManager contentManager, ModMetadata metadata, ModForLoading modForLoading)
        : IModLease
    {
        public ModMetadata Metadata => metadata;
        public bool IsLoaded => modForLoading.IsDone;
        public Mod LoadedMod => modForLoading.GetLoadedMod();

        public void Dispose()
        {
            contentManager.release(this);
        }
    }
}

interface IModLease : IDisposable
{
    bool IsLoaded { get; }
    Mod LoadedMod { get; }
}
