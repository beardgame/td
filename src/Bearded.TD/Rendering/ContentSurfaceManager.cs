using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics.Rendering;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Rendering.Deferred;

namespace Bearded.TD.Rendering
{
    sealed class ContentSurfaceManager : IDisposable
    {

        public LevelRenderer LevelRenderer { get; }
        public ImmutableArray<FluidGeometry> FluidGeometries { get; }

        private readonly Dictionary<SpriteDrawGroup, ReadOnlyCollection<IRenderer>> groupedAndSortedRenderers;

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
                        .Select(spriteSet => makeSpriteSetRenderer(context, spriteSet))
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
                foreach (var renderer in renderers)
                {
                    renderer.Render();
                }
            }
        }

        public void Dispose()
        {
            foreach (var drawGroup in groupedAndSortedRenderers.Values)
            {
                foreach (var renderer in drawGroup)
                {
                    renderer.Dispose();
                }
            }
        }
    }
}
