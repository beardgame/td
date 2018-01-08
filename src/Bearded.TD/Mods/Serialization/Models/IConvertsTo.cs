using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods.Serialization.Models
{
    interface IConvertsTo<out TGameModel, in TResolvers>
        where TGameModel : IBlueprint
    {
        TGameModel ToGameModel(TResolvers resolvers);
    }
}
