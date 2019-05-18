using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Rendering
{
    class GraphicsUnloader
    {
        public void CleanUp(Blueprints blueprints)
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
                        spriteSet.Sprites.Texture.Dispose();
                        spriteSet.Sprites.Surface.Dispose();
                    }
                );
        }
    }
}
