using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IWeaponJammer
{
    void Jam(TimeSpan duration);
}
