using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

interface IParticleParent : IPositionable
{
    string Name { get; }

    Velocity3 Velocity { get; }
}
