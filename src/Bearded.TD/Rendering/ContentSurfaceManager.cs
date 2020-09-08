using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;
using KeyValuePair = Bearded.TD.Utilities.Collections.KeyValuePair;

namespace Bearded.TD.Rendering
{
    class ContentSurfaceManager
    {
        public LevelRenderer LevelRenderer { get; }
        public ImmutableArray<FluidGeometry> FluidGeometries { get; }
        private static readonly ReadOnlyCollection<Surface> emptySurfaceList = new List<Surface>().AsReadOnly();

        private readonly Dictionary<SpriteDrawGroup, ReadOnlyCollection<Surface>> groupedAndSortedSpriteSets;

        public ContentSurfaceManager(LevelRenderer levelRenderer,
            ReadonlyBlueprintCollection<SpriteSet> spriteSets,
            IEnumerable<FluidGeometry> fluidGeometries)
        {
            LevelRenderer = levelRenderer;
            FluidGeometries = fluidGeometries.ToImmutableArray();
            groupedAndSortedSpriteSets = spriteSets.All
                .GroupBy(sprites => sprites.DrawGroup)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(sprites => sprites.DrawGroupOrderKey)
                        .Select(surfaceFromSpriteSet)
                        .ToList()
                        .AsReadOnly()
                );

            fillDictionaryForMissingGroups();
        }

        private Surface surfaceFromSpriteSet(SpriteSet spriteSet)
        {
            return spriteSet.Sprites.MeshBuilder;
        }

        private void fillDictionaryForMissingGroups()
        {
            var missingGroups = DeferredRenderer.AllDrawGroups
                .WhereNot(groupedAndSortedSpriteSets.ContainsKey);

            groupedAndSortedSpriteSets.AddRange(
                missingGroups.Select(g => KeyValuePair.From(g, emptySurfaceList))
                );
        }

        public ReadOnlyCollection<Surface> SurfacesFor(SpriteDrawGroup group)
        {
            return groupedAndSortedSpriteSets[group];
        }
    }
}
