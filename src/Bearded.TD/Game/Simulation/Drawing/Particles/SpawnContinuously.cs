using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Drawing.Particles.ParticleSpawning;

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
    private IToggle? toggle;

    public SpawnContinuously(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

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
        Particles.CreateParticles(Parameters, Velocity3.Zero, Owner.Direction, Owner.Game.Time, Owner.Position);
    }
}

