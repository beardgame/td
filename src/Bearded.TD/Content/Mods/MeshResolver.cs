using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Mods;

sealed class MeshResolver : IDependencyResolver<IMesh>
{
    private readonly ModMetadata thisMod;
    private readonly ReadonlyBlueprintCollection<Model> thisModsModels;
    private readonly ImmutableDictionary<string, Mod> mods;

    public MeshResolver(
        ModMetadata thisMod, ReadonlyBlueprintCollection<Model> thisModsModels, IEnumerable<Mod> otherMods)
    {
        this.thisMod = thisMod;
        this.thisModsModels = thisModsModels;
        mods = otherMods.ToImmutableDictionary(m => m.Id);
    }

    public IMesh Resolve(string id) => Resolve(ModAwareMeshId.FromNameInMod(id, thisMod));

    public IMesh Resolve(ModAwareMeshId id)
    {
        // TODO: consider sharing with sprite resolver and mod aware resolver
        var modId = id.Model.ModId;

        try
        {
            if (modId == null || modId == thisMod.Id)
            {
                return meshFrom(thisModsModels, id);
            }

            if (mods.TryGetValue(modId, out var mod))
            {
                return meshFrom(mod.Blueprints.Models, id);
            }
        }
        catch (Exception e)
        {
            throw new InvalidDataException($"Failed to find mesh with id \"{id}\".", e);
        }

        throw new InvalidDataException($"Unknown mod in identifier {id}");
    }


    private IMesh meshFrom(ReadonlyBlueprintCollection<Model> models, ModAwareMeshId id)
    {
        var meshId = id.Id;
        var model = models[id.Model];
        return model.GetMesh(meshId);
    }
}
