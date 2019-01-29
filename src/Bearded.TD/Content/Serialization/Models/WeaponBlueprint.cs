using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Weapons;
using Bearded.Utilities;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class WeaponBlueprint : IConvertsTo<Content.Models.WeaponBlueprint, Void>
    {
        public string Id { get; set; }
        public List<IComponent> Components { get; set; }

        public Content.Models.WeaponBlueprint ToGameModel(Void _)
        {
            return new Content.Models.WeaponBlueprint(
                Id,
                Components?.Select(ComponentFactories.CreateComponentFactory<Weapon>));
        }
    }
}
