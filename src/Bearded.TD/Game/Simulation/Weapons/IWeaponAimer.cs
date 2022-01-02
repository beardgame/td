using Bearded.Utilities.Geometry;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IWeaponAimer
{
    Direction2 AimDirection { get; }
}