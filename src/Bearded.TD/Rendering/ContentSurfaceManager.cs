﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Rendering
{
    class ContentSurfaceManager
    {
        public LevelGeometryManager LevelGeometry { get; }
        public ImmutableList<FluidGeometry> FluidGeometries { get; }
        private static readonly ReadOnlyCollection<Surface> emptySurfaceList = new List<Surface>().AsReadOnly();

        private readonly Dictionary<SpriteDrawGroup, ReadOnlyCollection<Surface>> groupedAndSortedSpriteSets;

        public ContentSurfaceManager(LevelGeometryManager levelGeometry,
            ReadonlyBlueprintCollection<SpriteSet> spriteSets,
            IEnumerable<FluidGeometry> fluidGeometries)
        {
            LevelGeometry = levelGeometry;
            FluidGeometries = fluidGeometries.ToImmutableList();
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
            return spriteSet.Sprites.Surface;
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
