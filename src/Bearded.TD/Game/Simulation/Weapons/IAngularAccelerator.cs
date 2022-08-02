using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IAngularAccelerator
{
    void Impact(AngularVelocity impact);
}
