using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class TurretParameters
    {
        public WeaponBlueprint Weapon { get; }

        [JsonConstructor]
        public TurretParameters(WeaponBlueprint weapon)
        {
            Weapon = weapon;
        }
    }
}
