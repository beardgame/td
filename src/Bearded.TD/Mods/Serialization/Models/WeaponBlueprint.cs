using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;
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
        public Unit Range { get; set; } = 5.U();
        public TimeSpan ReCalculateTilesInRangeInterval { get; set; } = 1.S();
        public TimeSpan NoTargetIdleInterval { get; set; } = 0.2.S();
        public List<IComponent> Components { get; set; }

        public Mods.Models.WeaponBlueprint ToGameModel(Void _)
        {
            return new Mods.Models.WeaponBlueprint(
                Id, Range, ReCalculateTilesInRangeInterval,
                NoTargetIdleInterval,
                Components?.Select(ComponentFactories.CreateComponentFactory<Weapon>));
        }
    }
}
