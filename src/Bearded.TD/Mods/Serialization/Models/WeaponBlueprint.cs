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
    class WeaponBlueprint : IConvertsTo<Mods.Models.WeaponBlueprint, Void>
    {
        public string Id { get; set; }
        public TimeSpan ShootInterval { get; set; } = new TimeSpan(0.15);
        public TimeSpan IdleInterval { get; set; } = new TimeSpan(0.3);
        public TimeSpan ReCalculateTilesInRangeInterval { get; set; } = 1.S();
        public Unit Range { get; set; } = 5.U();
        public int Damage { get; set; } = 10;
        public List<IComponent> Components { get; set; }

        public Mods.Models.WeaponBlueprint ToGameModel(Void _)
        {
            return new Mods.Models.WeaponBlueprint(
                    Id, ShootInterval, IdleInterval, ReCalculateTilesInRangeInterval, Range, Damage,
            Components?.Select(ComponentFactories.CreateComponentFactory<Weapon>));
        }
    }
}
