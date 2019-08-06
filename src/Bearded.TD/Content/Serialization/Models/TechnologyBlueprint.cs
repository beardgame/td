using System.Linq;
using Bearded.TD.Game.Technologies;
using Bearded.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    sealed class TechnologyBlueprint
        : IConvertsTo<Content.Models.TechnologyBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }

        public Content.Models.TechnologyBlueprint ToGameModel(Void resolvers)
        {
            return new Content.Models.TechnologyBlueprint(Id, Name, Cost, Enumerable.Empty<ITechnologyUnlock>());
        }
    }
}
