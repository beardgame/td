using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class BlueprintLoadingContext
    {
        public ModLoadingContext Context { get; }
        public ModMetadata Meta { get; }
        public JsonSerializer Serializer { get; }
        public ReadOnlyCollection<Mod> LoadedDependencies { get; }

        public BlueprintLoadingContext(ModLoadingContext context, ModMetadata meta, JsonSerializer serializer,
            ReadOnlyCollection<Mod> loadedDependencies)
        {
            Context = context;
            Meta = meta;
            Serializer = serializer;
            LoadedDependencies = loadedDependencies;
        }
    }
}
