
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods.Serialization.Models
{
    interface IConvertsTo<TGameModel, TResolvers>
        where TGameModel : IBlueprint
    {
        TGameModel ToGameModel(TResolvers resolvers);
    }
}
