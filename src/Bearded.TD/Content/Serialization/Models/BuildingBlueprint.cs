using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class BuildingBlueprint
        : IConvertsTo<Content.Models.BuildingBlueprint, UpgradeTagResolver>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<IBuildingComponent> Components { get; set; } = new();

        public Content.Models.BuildingBlueprint ToGameModel(ModMetadata modMetadata, UpgradeTagResolver tags)
        {
            return new(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Tags.Select(tags.Resolve),
                Components.Select(ComponentFactories.CreateBuildingComponentFactory)
            );
        }
    }
}
