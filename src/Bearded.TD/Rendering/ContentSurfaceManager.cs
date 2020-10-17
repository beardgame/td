using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Rendering;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Rendering.Deferred;

namespace Bearded.TD.Rendering
{
    using SpriteRenderers =
        ReadOnlyCollection<(IRenderer Renderer, ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> MeshBuilder)>;

    sealed class ContentSurfaceManager : IDisposable
    {
        public LevelRenderer LevelRenderer { get; }
        public ImmutableArray<FluidGeometry> FluidGeometries { get; }

        private readonly Dictionary<SpriteDrawGroup, SpriteRenderers> groupedAndSortedRenderers;

        public ContentSurfaceManager(RenderContext context, LevelRenderer levelRenderer,
            ReadonlyBlueprintCollection<SpriteSet> spriteSets,
            IEnumerable<FluidGeometry> fluidGeometries)
        {
            LevelRenderer = levelRenderer;
            FluidGeometries = fluidGeometries.ToImmutableArray();
            groupedAndSortedRenderers = spriteSets.All
                .GroupBy(sprites => sprites.DrawGroup)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(sprites => sprites.DrawGroupOrderKey)
                        .Select(spriteSet => (makeSpriteSetRenderer(context, spriteSet), spriteSet.Sprites.MeshBuilder))
                        .ToList()
                        .AsReadOnly()
                );
        }

        private IRenderer makeSpriteSetRenderer(RenderContext context, SpriteSet spriteSet)
        {
            return spriteSet.Sprites.CreateRendererWithSettings(
                context.Surfaces.ProjectionMatrix,
                context.Surfaces.ViewMatrix,
                context.Surfaces.FarPlaneDistance
            );
        }

        public void RenderDrawGroup(SpriteDrawGroup group)
        {
            if (groupedAndSortedRenderers.TryGetValue(group, out var renderers))
            {
                foreach (var (renderer, meshBuilder) in renderers)
                {
                    renderer.Render();
                    meshBuilder.Clear();
                }
            }
        }

        public void Dispose()
        {
            foreach (var drawGroup in groupedAndSortedRenderers.Values)
            {
                foreach (var (renderer, _) in drawGroup)
                {
                    renderer.Dispose();
                }
            }
        }
    }
}
