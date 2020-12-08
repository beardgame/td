using Bearded.TD.Content.Mods;
using Bearded.TD.Game.GameState;

namespace Bearded.TD.Content.Serialization.Models
{
    interface IConvertsTo<out TGameModel, in TResolvers>
        where TGameModel : IBlueprint
    {
        TGameModel ToGameModel(ModMetadata modMetadata, TResolvers resolvers);
    }
}
