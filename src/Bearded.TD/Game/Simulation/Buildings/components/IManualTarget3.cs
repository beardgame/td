using Bearded.TD.Game.Simulation.Weapons;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualTarget3 : IWeaponTrigger
{
    Position3 Target { get; }
}