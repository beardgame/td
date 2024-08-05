using System.IO;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Content.Mods
{
    sealed class ModelLoader
    {
        private readonly ModLoadingContext context;
        private readonly ModMetadata meta;

        public ModelLoader(ModLoadingContext context, ModMetadata meta)
        {
            this.context = context;
            this.meta = meta;
        }

        public Model TryLoad(FileInfo file, Content.Serialization.Models.Model jsonModel)
        {
            _ = jsonModel.Id ?? throw new InvalidDataException($"{nameof(jsonModel.Id)} must not be null");

            return new Model(ModAwareId.FromNameInMod(jsonModel.Id, meta));
        }
    }
}
