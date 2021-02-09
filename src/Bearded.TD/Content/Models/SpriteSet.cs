using System;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.Vertices;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Content.Models
{
    enum SpriteDrawGroup
    {
        // When adding new groups, make sure the DeferredRenderer knows about them, or they won't render
        LowResLevelDetail,
        LevelDetail,
        Building,
        Unit,
        Particle,
        Unknown
    }

    sealed class SpriteSet : IBlueprint, IDisposable
    {
        private readonly ISpriteSetImplementation sprites;
        public ModAwareId Id { get; }
        public SpriteDrawGroup DrawGroup { get; }
        public int DrawGroupOrderKey { get; }

        public SpriteSet(ModAwareId id, SpriteDrawGroup drawGroup, int drawGroupOrderKey, ISpriteSetImplementation sprites)
        {
            Id = id;
            DrawGroup = drawGroup;
            DrawGroupOrderKey = drawGroupOrderKey;
            this.sprites = sprites;
        }

        public ISpriteBlueprint GetSprite(string name)
        {
            return new SpriteBlueprint(this, name, sprites.GetSpriteParameters(name));
        }

        public DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
            SpriteRenderers spriteRenderers,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex)
            where TVertex : struct, IVertexData
        {
            return sprites.MakeConcreteWith(this, spriteRenderers, createVertex);
        }

        public (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
            SpriteRenderers spriteRenderers,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
            params IRenderSetting[] customRenderSettings)
            where TVertex : struct, IVertexData
        {
            return sprites.MakeCustomRendererWith(spriteRenderers, createVertex, customRenderSettings);
        }

        public void Dispose()
        {
            sprites.Dispose();
        }
    }
}
