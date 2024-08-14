using System.IO;
using Bearded.TD.Content.Models;
using SharpGLTF.Schema2;

namespace Bearded.TD.Content.Mods;

sealed class ModelLoader
{
    private readonly ModLoadingContext context;
    private readonly ModMetadata meta;

    public ModelLoader(ModLoadingContext context, ModMetadata meta)
    {
        this.context = context;
        this.meta = meta;
    }

    public Model Load(FileInfo file, Content.Serialization.Models.Model jsonModel)
    {
        _ = jsonModel.Id ?? throw new InvalidDataException($"{nameof(jsonModel.Id)} must not be null");

        var gltfFilePath = Path.ChangeExtension(file.FullName, "gltf");
        var modelRoot = ModelRoot.Load(gltfFilePath);

        context.Logger.Trace?.Log($"Loaded {gltfFilePath} with {modelRoot.LogicalMeshes.Count} meshes");

        return new Model(ModAwareId.FromNameInMod(jsonModel.Id, meta));
    }
}
