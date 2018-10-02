using Bearded.TD.Game.Weapons;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface ITurretParameters : IParametersTemplate
    {
        [Modifiable] IWeaponBlueprint Weapon { get; }
        [Modifiable] Difference2 Offset { get; }
    }
}
