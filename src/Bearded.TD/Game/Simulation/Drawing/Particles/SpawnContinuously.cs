using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Drawing.Particles.ParticleSpawning;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnContinuously")]
sealed class SpawnContinuously : ParticleUpdater<SpawnContinuously.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        [Modifiable(1)]
        TimeSpan Interval { get; }
        float IntervalNoise { get; }

        string? Toggle { get; }
    }

    private Instant nextSpawn;
    private IMoving? moving;
    private IToggle? toggle;

    public SpawnContinuously(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

        ComponentDependencies.Depend<IMoving>(Owner, Events, m => moving = m);

        if (Parameters.Toggle is not { } name)
            return;

        ComponentDependencies.Depend<IToggle>(Owner, Events, t => toggle = t, t => t.Name == name);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (toggle is { Enabled: false })
            return;

        if (nextSpawn <= Owner.Game.Time)
        {
            spawn();
            nextSpawn = Owner.Game.Time + Parameters.Interval * Noise(Parameters.IntervalNoise);
        }
    }

    private void spawn()
    {
        var v = moving?.Velocity ?? Velocity3.Zero;
        Particles.CreateParticles(Parameters, v, Owner.Direction, Owner.Game.Time, Owner.Position);
    }
}

