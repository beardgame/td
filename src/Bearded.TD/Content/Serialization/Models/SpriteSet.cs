using System;
using System.Collections.Generic;
using System.IO;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content.Serialization.Models
{
    class SpriteSet : IConvertsTo<Content.Models.SpriteSet, (FileInfo, SpriteSetLoader)>
    {
        public class TextureSampler
        {
            public string Sampler { get; set; }
            public string Prefix { get; set; }
            public string Suffix { get; set; }
        }

        public string Id { get; set; }
        public SpriteDrawGroup DrawGroup { get; set; } = SpriteDrawGroup.Unknown;
        public int DrawGroupOrderKey { get; set; }

        public Content.Models.Shader Shader { get; set; }

        public string DefaultTextureSampler { get; set; }
        public List<TextureSampler> TextureSamplers { get; set; }

        public bool Pixelate { get; set; }

        public Content.Models.SpriteSet ToGameModel((FileInfo, SpriteSetLoader) resolvers)
        {
            if (Shader == null)
                throw new Exception("Cannot load sprite set without shader.");

            var (file, loader) = resolvers;

            return loader.TryLoad(file, this);
        }
    }
}
