using Bearded.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

interface IParticleSpawnParameters
{
    [Modifiable(1)]
    int Count { get; }

    TimeSpan? LifeTime { get; }
    float LifeTimeNoise { get; }

    [Modifiable(1)]
    Unit Size { get; }
    float SizeNoise { get; }

    Difference3 Offset { get; }
    bool RelativeToDirection { get; }

    Color? Color { get; }

    Velocity3 Velocity { get; }
    float VelocityNoise { get; }

    Speed RandomVelocity { get; }
    float RandomVelocityNoise { get; }

    Angle Orientation { get; }
    float OrientationNoise { get; }

    AngularVelocity AngularVelocity { get; }
    float AngularVelocityNoise { get; }
    bool RandomAngularVelocitySign { get; }
}
