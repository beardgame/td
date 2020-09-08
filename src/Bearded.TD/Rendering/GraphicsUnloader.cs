using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Rendering
{
    static class GraphicsUnloader
    {
        public static void CleanUp(Blueprints blueprints)
        {
            cleanUp(blueprints.Materials);
            cleanUp(blueprints.Sprites);
        }

        private static void cleanUp(ReadonlyBlueprintCollection<Material> materials)
        {
            materials.All
                .SelectMany(m => m.ArrayTextures)
                .ForEach(textureUniform => textureUniform.Texture.Dispose());
        }

        private static void cleanUp(ReadonlyBlueprintCollection<SpriteSet> spriteSets)
        {
            spriteSets.All
                .ForEach(spriteSet =>
                    {
                        spriteSet.Sprites.Textures.ForEach(t => t.Dispose());
                        spriteSet.Sprites.MeshBuilder.Dispose();
                    }
                );
        }
    }
}
