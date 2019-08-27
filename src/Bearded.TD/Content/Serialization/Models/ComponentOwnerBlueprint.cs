using System.Collections.Generic;
using Bearded.Utilities;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class ComponentOwnerBlueprint : IConvertsTo<Content.Models.ComponentOwnerBlueprint, Void>
    {
        public string Id { get; set; }
        public List<IComponent> Components { get; set; }

        public Content.Models.ComponentOwnerBlueprint ToGameModel(Void _)
        {
            return new Content.Models.ComponentOwnerBlueprint(Id, Components);
        }
    }
}
