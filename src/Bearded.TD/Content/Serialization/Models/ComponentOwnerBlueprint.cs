using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class ComponentOwnerBlueprint : IConvertsTo<Content.Models.GameObjectBlueprint, Void>
{
    public string? Id { get; set; }
    public List<IComponent>? Components { get; set; }

    public Content.Models.GameObjectBlueprint ToGameModel(ModMetadata modMetadata, Void v)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");

        return new(ModAwareId.FromNameInMod(Id, modMetadata), Components ?? Enumerable.Empty<IComponent>());
    }
}
