using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class NodeBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<INodeBlueprint, NodeBlueprint, Void>(context)
{
    protected override string RelativePath => "defs/levelnodes";
    protected override DependencySelector SelectDependency => m => m.Blueprints.LevelNodes;
}
