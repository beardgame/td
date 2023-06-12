using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Weapons;

[Trigger("weaponMisfired")]
readonly record struct WeaponMisfired : IComponentEvent;
