using System.Linq;
using Bearded.TD.Game.Upgrades;
using Bearded.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    sealed class UpgradeBlueprint
        : IConvertsTo<Content.Models.UpgradeBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }

        public Content.Models.UpgradeBlueprint ToGameModel(Void resolvers)
        {
            return new Content.Models.UpgradeBlueprint(Id, Name, Cost, Enumerable.Empty<IUpgradeEffect>());
        }
    }
}
