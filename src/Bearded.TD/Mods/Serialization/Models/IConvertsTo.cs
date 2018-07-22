using Bearded.TD.Game;

namespace Bearded.TD.Mods.Serialization.Models
{
    interface IConvertsTo<out TGameModel, in TResolvers>
        where TGameModel : IBlueprint
    {
        TGameModel ToGameModel(TResolvers resolvers);
    }
}
