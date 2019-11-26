using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game;
using Bearded.Utilities;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class SimpleBlueprintLoader<TBlueprint, TJsonModel> : BaseBlueprintLoader<TBlueprint, TJsonModel, Void>
        where TBlueprint : IBlueprint
        where TJsonModel : IConvertsTo<TBlueprint, Void>
    {
        protected override string RelativePath { get; }

        public SimpleBlueprintLoader(BlueprintLoadingContext context, string relativePath) : base(context)
        {
            RelativePath = relativePath;
        }
    }
}
