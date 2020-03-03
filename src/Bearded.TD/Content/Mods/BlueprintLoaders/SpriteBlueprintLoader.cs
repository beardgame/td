using System.IO;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Serialization.Converters;
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
            loader = new SpriteSetLoader(context.Context);
        }

        protected override void SetupDependencyResolver(ReadonlyBlueprintCollection<SpriteSet> blueprintCollection)
        {
            var dependencyResolver = new SpriteResolver(Context.Meta, blueprintCollection, Context.LoadedDependencies);
            Context.Serializer.Converters.Add(new DependencyConverter<ISprite>(dependencyResolver));
        }

        protected override (FileInfo, SpriteSetLoader) GetDependencyResolvers(FileInfo file)
        {
            return (file, loader);
        }
    }
}
