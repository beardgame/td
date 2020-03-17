using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Game.Upgrades;
using Bearded.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    sealed class UpgradeBlueprint
        : IConvertsTo<Content.Models.UpgradeBlueprint, Void>
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Cost { get; set; }
        public List<IUpgradeEffect> Effects { get; set; } = new List<IUpgradeEffect>();

        public Content.Models.UpgradeBlueprint ToGameModel(Void resolvers)
        {
            return new Content.Models.UpgradeBlueprint(Id, Name, Cost, ImmutableList.CreateRange(Effects));
        }
    }
}
