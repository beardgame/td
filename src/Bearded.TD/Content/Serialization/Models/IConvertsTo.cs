using System.IO;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Serialization.Models
{
    interface IConvertsTo<out TGameModel, in TResolvers>
        where TGameModel : IBlueprint
    {
        TGameModel ToGameModel(TResolvers resolvers);
    }
}
