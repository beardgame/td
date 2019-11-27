using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Technologies;

namespace Bearded.TD.Content.Serialization.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    sealed class TechnologyBlueprint
        : IConvertsTo<Content.Models.TechnologyBlueprint, IDependencyResolver<ITechnologyBlueprint>>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public List<ITechnologyUnlock> Unlocks { get; set; }
        public List<string> RequiredTechs { get; set; } = new List<string>();

        public Content.Models.TechnologyBlueprint ToGameModel(IDependencyResolver<ITechnologyBlueprint> resolvers)
        {
            return new Content.Models.TechnologyBlueprint(
                Id,
                Name,
                Cost,
                ImmutableList.CreateRange(Unlocks),
                RequiredTechs.Select(resolvers.Resolve));
        }
    }
}
