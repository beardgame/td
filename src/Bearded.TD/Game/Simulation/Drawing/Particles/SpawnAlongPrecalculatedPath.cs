using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnAlongPrecalculatedPath")]
sealed class SpawnAlongPrecalculatedPath : ParticleUpdater<SpawnAlongPrecalculatedPath.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        [Modifiable(0.1f)]
        Unit Z { get; }
    }

    public SpawnAlongPrecalculatedPath(IParameters parameters) : base(parameters) { }

    public override void Activate()
    {
        base.Activate();

        if (!Owner.TryGetProperty<PrecalculatedPath>(out var path))
        {
            Owner.Game.Meta.Logger.Debug?
                .Log($"{nameof(SpawnAlongPrecalculatedPath)} requires a precalculated path property.");
            return;
        }

        var pathAsTiles = path.ToTileList();
        var particleCount = pathAsTiles.Length;
        var particles = Particles.CreateParticles(
            Parameters,
            Velocity3.Zero,
            Direction2.Zero,
            Owner.Game.Time,
            Position3.Zero,
            out var transaction,
            particleCount);

        for (var i = 0; i < particleCount; i++)
        {
            particles[i].Position = Level.GetPosition(pathAsTiles[i]).WithZ(Parameters.Z);
        }

        transaction.Commit();
    }

    public override void Update(TimeSpan elapsedTime) { }
}
