using Bearded.Utilities;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

using FontDefinition = Models.Fonts.FontDefinition;
using FontDefinitionJson = Serialization.Models.Fonts.FontDefinition;

sealed class FontDefinitionBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<FontDefinition, FontDefinitionJson, Void>(context)
{
    protected override string RelativePath => "gfx/fonts/definitions";
    protected override DependencySelector SelectDependency => m => m.Blueprints.FontDefinitions;
}
