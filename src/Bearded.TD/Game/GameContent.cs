using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game;

sealed class GameContent(ContentManager contentManager)
{
    private readonly HashSet<ModMetadata> enabledMods = [];
    private readonly Dictionary<ModMetadata, IModLease> leasesByMod = new();

    public IEnumerable<ModMetadata> AvailableMods => contentManager.VisibleMods;
    public ImmutableHashSet<ModMetadata> EnabledMods => enabledMods.ToImmutableHashSet();

    public bool IsFinishedLoading => enabledMods.Select(mod => leasesByMod[mod]).All(l => l.IsLoaded);
    public ModLoadingProfiler LoadingProfiler => contentManager.LoadingProfiler;

    public ModMetadata FindMod(string id) => contentManager.FindMod(id);

    public void SetEnabledModsById(IEnumerable<string> modIds)
    {
        enabledMods.Clear();
        modIds.Select(contentManager.FindMod).ForEach(enableMod);
    }

    private void enableMod(ModMetadata mod)
    {
        if (enabledMods.Contains(mod))
        {
            return;
        }

        // Make sure all dependencies are enabled too.
        foreach (var dependency in mod.Dependencies)
        {
            var dependencyMod = contentManager.FindMod(dependency.Id);
            if (!enabledMods.Contains(dependencyMod))
            {
                enableMod(dependencyMod);
            }
        }

        // Enqueue for loading if it hasn't been loaded yet.
        if (!leasesByMod.ContainsKey(mod))
        {
            var lease = contentManager.LeaseMod(mod);
            leasesByMod.Add(mod, lease);
        }

        enabledMods.Add(mod);
    }

    public HashSet<ModMetadata> PreviewEnableMod(ModMetadata modToEnable)
    {
        var enabled = enabledMods.ToHashSet();

        enable(modToEnable);

        return enabled;

        void enable(ModMetadata mod)
        {
            if (enabled.Contains(mod))
                return;

            foreach (var dependency in mod.Dependencies)
            {
                var dependencyMod = contentManager.FindMod(dependency.Id);
                enable(dependencyMod);
            }

            enabled.Add(mod);
        }
    }

    public HashSet<ModMetadata> PreviewDisableMod(ModMetadata modToDisable)
    {
        var enabled = enabledMods.ToHashSet();

        disable(modToDisable);

        return enabled;

        void disable(ModMetadata mod)
        {
            if (!enabled.Contains(mod))
                return;

            var dependents = enabled.Where(m => m.Dependencies.Any(d => d.Id == mod.Id));
            dependents.ForEach(disable);

            enabled.Remove(mod);
        }
    }

    public IEnumerable<IGameModeBlueprint> ListGameModes()
    {
        DebugAssert.State.Satisfies(IsFinishedLoading);
        return enabledMods.SelectMany(mod => leasesByMod[mod].LoadedMod.Blueprints.GameModes.All);
    }

    public Blueprints CreateBlueprints()
    {
        DebugAssert.State.Satisfies(IsFinishedLoading);
        return Blueprints.Merge(enabledMods.Select(mod => leasesByMod[mod].LoadedMod.Blueprints));
    }

    public void CleanUpUnused()
    {
        var unusedMods = leasesByMod.Where(kvp => !enabledMods.Contains(kvp.Key)).ToList();
        foreach (var (metadata, lease) in unusedMods)
        {
            lease.Dispose();
            leasesByMod.Remove(metadata);
        }
        contentManager.CleanUpUnused();
    }

    public void Dispose()
    {
        leasesByMod.Values.ForEach(l => l.Dispose());
        enabledMods.Clear();
        leasesByMod.Clear();
        contentManager.CleanUpAll();
    }

    [Obsolete]
    public void Update()
    {
        contentManager.Update();
    }
}
