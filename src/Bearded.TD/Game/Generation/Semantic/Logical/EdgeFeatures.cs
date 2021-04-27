using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.Logical
{
    sealed record EdgeFeatures(bool IsConnected, MacroFeature? Feature);
}
