using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class TurretParameters
    {
        public WeaponBlueprint Weapon { get; }
        public Difference2 Offset { get; }

        [JsonConstructor]
        public TurretParameters(WeaponBlueprint weapon, Difference2? offset = null)
        {
            Weapon = weapon;
            Offset = offset ?? Difference2.Zero;
        }
    }
}
