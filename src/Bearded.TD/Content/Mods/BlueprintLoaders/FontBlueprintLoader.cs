using Bearded.Utilities;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

using Font = Models.Fonts.Font;
using FontJson = Serialization.Models.Fonts.Font;

sealed class FontBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<Font, FontJson, Void>(context)
{
    protected override string RelativePath => "gfx/fonts/fonts";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Fonts;
}
