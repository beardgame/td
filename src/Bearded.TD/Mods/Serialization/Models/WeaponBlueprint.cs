using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class WeaponBlueprint : IConvertsTo<Mods.Models.WeaponBlueprint, Void>
    {
        public string Id { get; set; }
        public List<IComponent> Components { get; set; }

        public Mods.Models.WeaponBlueprint ToGameModel(Void _)
        {
            return new Mods.Models.WeaponBlueprint(
                Id,
                Components?.Select(ComponentFactories.CreateComponentFactory<Weapon>));
        }
    }
}
