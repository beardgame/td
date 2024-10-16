﻿using System.Collections.Generic;
using System.IO;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class SpriteSet : IConvertsTo<Content.Models.SpriteSet, (FileInfo, SpriteSetLoader)>
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TextureSampler
    {
        public string? Sampler { get; set; }
        public string? Prefix { get; set; }
        public string? Suffix { get; set; }
        public List<string>? Transformations { get; set; }
    }

    public string? Id { get; set; }
    public DrawOrderGroup DrawGroup { get; set; } = DrawOrderGroup.Unknown;
    public int DrawGroupOrderKey { get; set; }

    public string? DefaultTextureSampler { get; set; }
    public List<TextureSampler>? TextureSamplers { get; set; }

    public SpritePackMode PackMode { get; set; } = SpritePackMode.Any;
    public int Padding { get; set; }

    public bool PreMultiply { get; set; } = true;

    public Content.Models.SpriteSet ToGameModel(ModMetadata contextMeta, (FileInfo, SpriteSetLoader) resolvers)
    {
        var (file, loader) = resolvers;

        return loader.TryLoad(file, this);
    }
}
