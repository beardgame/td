using Bearded.Graphics;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

struct Particle
{
    public Position3 Position;
    public Velocity3 Velocity;
    public float Size;
    public Direction2 Direction;
    public AngularVelocity AngularVelocity;
    public Color Color;
    public Instant CreationTime;
    public Instant? TimeOfDeath;
    public bool Deleted;

    public bool IsAliveAtTime(Instant time)
        => !Deleted && (TimeOfDeath == null || time < TimeOfDeath);

    public float AgeFactorAtTime(Instant time)
        => TimeOfDeath is { } timeOfDeath
            ? (float) ((time - CreationTime) / (timeOfDeath - CreationTime))
            : 0;
}
