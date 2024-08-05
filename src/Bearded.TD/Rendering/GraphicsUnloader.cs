using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Rendering;

static class GraphicsUnloader
{
    public static void CleanUp(Blueprints blueprints)
    {
        cleanUp(blueprints.Materials);
        cleanUp(blueprints.Sprites);
        cleanUp(blueprints.Models);
    }

    private static void cleanUp(ReadonlyBlueprintCollection<Material> materials)
    {
        materials.All.SelectMany(m => m.Textures).ForEach(t => t.Texture.Dispose());
    }

    private static void cleanUp(ReadonlyBlueprintCollection<SpriteSet> spriteSets)
    {
        spriteSets.All.ForEach(spriteSet => spriteSet.Dispose());
    }

    private static void cleanUp(ReadonlyBlueprintCollection<Model> models)
    {
        models.All.ForEach(model => model.Dispose());
    }
}
