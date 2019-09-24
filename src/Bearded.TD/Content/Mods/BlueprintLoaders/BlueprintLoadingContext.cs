using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class BlueprintLoadingContext
    {
        public ModLoadingContext Context { get; }
        public ModMetadata Meta { get; }
        public JsonSerializer Serializer { get; }

        public BlueprintLoadingContext(ModLoadingContext context, ModMetadata meta, JsonSerializer serializer)
        {
            Context = context;
            Meta = meta;
            Serializer = serializer;
        }
    }
}
