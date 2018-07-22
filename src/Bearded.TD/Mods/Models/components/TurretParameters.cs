using Bearded.TD.Game.Weapons;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class TurretParameters
    {
        public IWeaponBlueprint Weapon { get; }
        public Difference2 Offset { get; }

        [JsonConstructor]
        public TurretParameters(WeaponBlueprint weapon, Difference2? offset = null)
        {
            Weapon = weapon;
            Offset = offset ?? Difference2.Zero;
        }
    }
}
