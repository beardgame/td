using System.Collections.Generic;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class ComponentOwnerBlueprint : IConvertsTo<Content.Models.ComponentOwnerBlueprint, Void>
    {
        public string Id { get; set; }
        public List<IComponent> Components { get; set; }

        public Content.Models.ComponentOwnerBlueprint ToGameModel(ModMetadata modMetadata, Void _)
        {
            return new(ModAwareId.FromNameInMod(Id, modMetadata), Components);
        }
    }
}
