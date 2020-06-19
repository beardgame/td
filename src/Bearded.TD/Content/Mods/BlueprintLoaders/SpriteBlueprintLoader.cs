using System.IO;
using Bearded.TD.Game;
using SpriteSet = Bearded.TD.Content.Models.SpriteSet;
using SpriteSetJson = Bearded.TD.Content.Serialization.Models.SpriteSet;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class SpriteBlueprintLoader : BaseBlueprintLoader<SpriteSet, SpriteSetJson, (FileInfo, SpriteSetLoader)>
    {
        private readonly SpriteSetLoader loader;

        protected override string RelativePath => "gfx/sprites";

        public SpriteBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
            loader = new SpriteSetLoader(context.Context, context.Meta);
        }

        protected override void SetupDependencyResolver(ReadonlyBlueprintCollection<SpriteSet> blueprintCollection)
        {
            Context.AddDependencyResolver(
                new SpriteResolver(Context.Meta, blueprintCollection, Context.LoadedDependencies));
        }

        protected override (FileInfo, SpriteSetLoader) GetDependencyResolvers(FileInfo file)
        {
            return (file, loader);
        }
    }
}
