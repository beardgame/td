using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class TurretParameters
    {
        public WeaponParameters Weapon { get; }

        [JsonConstructor]
        public TurretParameters(WeaponParameters weapon)
        {
            Weapon = weapon;
        }
    }
}
