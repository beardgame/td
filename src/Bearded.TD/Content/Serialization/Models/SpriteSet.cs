using System;
using System.IO;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content.Serialization.Models
{
    class SpriteSet : IConvertsTo<Content.Models.SpriteSet, (FileInfo, SpriteSetLoader)>
    {
        public string? Id { get; set; }
        public SpriteDrawGroup DrawGroup { get; set; } = SpriteDrawGroup.Unknown;
        public int DrawGroupOrderKey { get; set; }

        public Content.Models.Shader? Shader { get; set; }

        public string? DefaultTextureSampler { get; set; }
        public bool Pixelate { get; set; }

        public Content.Models.SpriteSet ToGameModel((FileInfo, SpriteSetLoader) resolvers)
        {
            if (Shader == null)
                throw new Exception("Cannot load sprite set without shader.");
            if (DefaultTextureSampler == null)
                throw new Exception("Cannot load sprite set without default texture sampler.");

            var (file, loader) = resolvers;

            return loader.TryLoad(file, this);
        }
    }
}
