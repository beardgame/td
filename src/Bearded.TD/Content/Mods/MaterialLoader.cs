using System;
using System.IO;
using Bearded.TD.Content.Models;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods
{
    class MaterialLoader
    {
        private readonly ModLoadingContext context;
        private readonly ModMetadata meta;

        public MaterialLoader(ModLoadingContext context, ModMetadata meta)
        {
            this.context = context;
            this.meta = meta;
        }

        public Material TryLoad(FileInfo file, Serialization.Models.Material materialJson)
        {
            return null;
        }
    }
}
