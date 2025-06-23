using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("spinningParticleParent")]
sealed class SpinningParticleParent : Component<SpinningParticleParent.IParameters>, IParticleParent
{
    public enum Axis
    {
        X,
        Y,
        Z,
        AlongVelocity,
    }

    public interface IParameters : IParametersTemplate<IParameters>
    {
        string Name { get; }

        Unit Radius { get; }
        Axis Axis { get; }

        AngularVelocity AngularVelocity { get; }
        float AngularVelocityNoise { get; }
        bool RandomSign { get; }
    }

    private readonly Speed orbitalSpeed;
    private IMoving? moving;

    private bool initialised;
    private Difference3 currentOffset;

    public string Name => Parameters.Name;
    public Position3 Position { get; private set; }
    public Velocity3 Velocity { get; private set; }

    public SpinningParticleParent(IParameters parameters) : base(parameters)
    {
        var anglePerSecond = Parameters.AngularVelocity * 1.S();

        orbitalSpeed = anglePerSecond.Radians * Parameters.Radius / 1.S()
            * ParticleSpawning.Noise(Parameters.AngularVelocityNoise);

        if (Parameters.RandomSign)
            orbitalSpeed *= Random.Shared.NextSign();
    }

    public override void Activate()
    {
        base.Activate();

        ComponentDependencies.Depend<IMoving>(Owner, Events, m => moving = m);
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var axis = Parameters.Axis switch
        {
            Axis.X => Vector3.UnitX,
            Axis.Y => Vector3.UnitY,
            Axis.Z => Vector3.UnitZ,
            Axis.AlongVelocity => moving?.Velocity.NumericValue.NormalizedSafe() ?? Vector3.UnitZ,
            _ => Vector3.UnitZ,
        };

        if (!initialised)
            initialiseWith(axis);

        Velocity = orbitalSpeed * Vector3.Cross(axis, currentOffset.NumericValue);

        currentOffset += Velocity * elapsedTime;
        currentOffset = currentOffset.NumericValue.NormalizedSafe() * Parameters.Radius;

        Position = Owner.Position + currentOffset;
    }

    private void initialiseWith(Vector3 axis)
    {
        var nonParallelVector = Math.Abs(axis.X) < 0.99f ? Vector3.UnitX : Vector3.UnitY;

        var u = Vector3.Cross(axis, nonParallelVector).Normalized();
        var v = Vector3.Cross(axis, u).Normalized();

        var angle = Random.Shared.NextFloat(MathF.Tau);

        currentOffset = Parameters.Radius * (u * MathF.Cos(angle) + v * MathF.Sin(angle));

        initialised = true;
    }
}
