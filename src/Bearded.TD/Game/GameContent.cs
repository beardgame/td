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
    private readonly Dictionary<ModMetadata, IModReference> referencesByMod = new();

    public IEnumerable<ModMetadata> AvailableMods => contentManager.VisibleMods;
    public ImmutableHashSet<ModMetadata> EnabledMods => enabledMods.ToImmutableHashSet();

    public bool IsFinishedLoading => enabledMods.Select(mod => referencesByMod[mod]).All(l => l.IsLoaded);
    public ModLoadingProfiler LoadingProfiler => contentManager.LoadingProfiler;

    public ModMetadata FindMod(string id) => contentManager.FindMetadata(id);

    public void SetEnabledModsById(IEnumerable<string> modIds)
    {
        enabledMods.Clear();
        modIds.Select(contentManager.FindMetadata).ForEach(enableMod);
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
            var dependencyMod = contentManager.FindMetadata(dependency.Id);
            if (!enabledMods.Contains(dependencyMod))
            {
                enableMod(dependencyMod);
            }
        }

        // Create mod reference if it doesn't exist yet.
        if (!referencesByMod.ContainsKey(mod))
        {
            var reference = contentManager.ReferenceMod(mod);
            referencesByMod.Add(mod, reference);
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
                var dependencyMod = contentManager.FindMetadata(dependency.Id);
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
        return enabledMods.SelectMany(mod => referencesByMod[mod].LoadedMod.Blueprints.GameModes.All);
    }

    public Blueprints CreateBlueprints()
    {
        DebugAssert.State.Satisfies(IsFinishedLoading);
        return Blueprints.Merge(enabledMods.Select(mod => referencesByMod[mod].LoadedMod.Blueprints));
    }

    public void CleanUpUnused()
    {
        var unusedMods = referencesByMod.Where(kvp => !enabledMods.Contains(kvp.Key)).ToList();
        foreach (var (metadata, reference) in unusedMods)
        {
            reference.Dispose();
            referencesByMod.Remove(metadata);
        }
        contentManager.CleanUpUnused();
    }

    public void Dispose()
    {
        referencesByMod.Values.ForEach(l => l.Dispose());
        enabledMods.Clear();
        referencesByMod.Clear();
        contentManager.CleanUpUnused();
    }
}
