﻿using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.TD.Game;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed class BiomeMap : LevelTextureMap
{
    public BiomeMap(GameInstance game)
        : base(game, "biomemap",
            PixelInternalFormat.Rgb8,
            t => t.SetFilterMode(TextureMinFilter.Nearest, TextureMagFilter.Nearest))
    {
    }

    public IPipeline<Void> DrawBiomeIndex(IPipeline<Void> pipeline)
        => DrawWithMask(pipeline, ColorMask.DrawAll);
}
