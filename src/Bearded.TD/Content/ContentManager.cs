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
    private readonly MultiDictionary<ModMetadata, ModReference> referencesByMod = new();

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

    public ModMetadata FindMetadata(string modId) => modsById[modId];

    public Mod GetModUnsafe(string modId)
    {
        var metadata = FindMetadata(modId);
        if (!modsForLoading.TryGetValue(metadata, out var modForLoading) || !modForLoading.IsDone)
        {
            throw new InvalidOperationException("Cannot access a mod that isn't loaded");
        }

        return modForLoading.GetLoadedMod();
    }

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
        } while (metadata != null && !modsForLoading.ContainsKey(metadata));

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
        foreach (var metadata in listUnusedMods())
        {
            // We have no way to abort loading. Just finish loading it and we'll pick it up in a future clean-up cycle.
            if (currentlyLoading == metadata)
            {
                continue;
            }

            var modForLoading = modsForLoading[metadata];
            if (modForLoading.IsDone)
            {
                GraphicsUnloader.CleanUp(modForLoading.GetLoadedMod().Blueprints);
            }
            modsForLoading.Remove(metadata);
        }
    }

    private ImmutableArray<ModMetadata> listUnusedMods()
    {
        var usedMods = new HashSet<ModMetadata>();
        foreach (var (key, _) in referencesByMod)
        {
            visitMod(key);
        }
        return modsForLoading.Keys.WhereNot(usedMods.Contains).ToImmutableArray();

        void visitMod(ModMetadata mod)
        {
            if (!usedMods.Add(mod))
            {
                return;
            }

            foreach (var dep in mod.Dependencies.Select(d => FindMetadata(d.Id)))
            {
                visitMod(dep);
            }
        }
    }

    public void CleanUpAll()
    {
        DebugAssert.State.Satisfies(isFinishedLoading);
        foreach (var (_, modForLoading) in modsForLoading)
        {
            GraphicsUnloader.CleanUp(modForLoading.GetLoadedMod().Blueprints);
        }
        referencesByMod.Clear();
        modsForLoading.Clear();
    }

    public IModReference ReferenceMod(ModMetadata mod)
    {
        var reference = new ModReference(this, mod, findModForLoading(mod));
        referencesByMod.Add(mod, reference);
        return reference;
    }

    private ModForLoading findModForLoading(ModMetadata metadata)
    {
        return modsForLoading.TryGetValue(metadata, out var mod) ? mod : queueModForLoading(metadata);
    }

    private ModForLoading queueModForLoading(ModMetadata metadata)
    {
        foreach (var dep in metadata.Dependencies.Select(d => FindMetadata(d.Id)))
        {
            findModForLoading(dep);
        }

        var m = new ModForLoading(metadata);
        modsForLoading.Add(metadata, m);
        modLoadingQueue.Enqueue(metadata);
        return m;
    }

    private void dereference(ModReference reference)
    {
        referencesByMod.Remove(reference.Metadata, reference);
    }

    private sealed class ModReference(ContentManager contentManager, ModMetadata metadata, ModForLoading modForLoading)
        : IModReference
    {
        public ModMetadata Metadata => metadata;
        public bool IsLoaded => modForLoading.IsDone;
        public Mod LoadedMod => modForLoading.GetLoadedMod();

        public void Dispose()
        {
            contentManager.dereference(this);
        }
    }
}

interface IModReference : IDisposable
{
    bool IsLoaded { get; }
    Mod LoadedMod { get; }
}
