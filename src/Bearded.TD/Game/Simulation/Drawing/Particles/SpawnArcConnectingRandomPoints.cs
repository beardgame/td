using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.Game.Simulation.Drawing.Particles.ParticleSpawning;
using static Bearded.Utilities.MoreMath;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnArcConnectingRandomPoints")]
sealed class SpawnArcConnectingRandomPoints : ParticleUpdater<SpawnArcConnectingRandomPoints.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        [Modifiable(0.25)]
        Unit SegmentLength { get; }

        float Curvature { get; }
        float CurvatureNoise { get; }

        bool ZeroEndPointVelocity { get; }

        ImmutableArray<Difference3> Points { get; }

        [Modifiable(1)]
        TimeSpan Interval { get; }
        float IntervalNoise { get; }

        string? Toggle { get; }
    }

    private Instant nextSpawn;
    private IToggle? toggle;

    public SpawnArcConnectingRandomPoints(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

        if (Parameters.Points.IsDefaultOrEmpty || Parameters.Points.Length < 2)
        {
            Owner.Game.Meta.Logger.Debug?.Log($"{nameof(SpawnArcConnectingRandomPoints)} needs at least two points.");
            nextSpawn = new Instant(double.MaxValue);
            return;
        }

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
        var startIndex = StaticRandom.Int(Parameters.Points.Length);
        var endIndex = StaticRandom.Int(Parameters.Points.Length - 1);
        if (endIndex == startIndex)
            endIndex = Parameters.Points.Length - 1;

        var start = Parameters.Points[startIndex];
        var end = Parameters.Points[endIndex];

        var unitY = Owner.Direction.Vector;
        var unitX = unitY.PerpendicularRight;

        var startPoint = Owner.Position + (start.X * unitX + start.Y * unitY).WithZ(start.Z);
        var endPoint = Owner.Position + (end.X * unitX + end.Y * unitY).WithZ(end.Z);

        var startToEndXY = endPoint.XY() - startPoint.XY();
        var startToEndDirection = Direction2.Of(startToEndXY.NumericValue);
        var startToEndDistance = startToEndXY.Length;

        var curvature = Parameters.Curvature * Noise(Parameters.CurvatureNoise);

        var bezierControlDistance = startToEndDistance * curvature * (2f / 3f);
        var bezierControlAngle = curvature * 90.Degrees();

        var controlPoint1 = startPoint.XY() + (startToEndDirection + bezierControlAngle) * bezierControlDistance;
        var controlPoint2 = endPoint.XY() + (-startToEndDirection - bezierControlAngle) * bezierControlDistance;

        var bezier = new BezierCurveCubic(
            startPoint.XY().NumericValue,
            endPoint.XY().NumericValue,
            controlPoint1.NumericValue,
            controlPoint2.NumericValue);

        var bezierLength = bezier.CalculateLength(0.1f);

        var particleCount = Max(2, CeilToInt(bezierLength / Parameters.SegmentLength.NumericValue));
        var step = 1f / particleCount;

        var particles = Particles.CreateParticles(
            Parameters, Velocity3.Zero, Direction2.Zero, Owner.Game.Time, Position3.Zero,
            out var transaction, particleCount);

        particles[0].Position = startPoint;
        particles[^1].Position = endPoint;

        if (Parameters.ZeroEndPointVelocity)
        {
            particles[0].Velocity = Velocity3.Zero;
            particles[^1].Velocity = Velocity3.Zero;
        }

        var zDiff = endPoint.Z - startPoint.Z;

        for (var i = 1; i < particleCount - 1; i++)
        {
            var t = i * step;
            var position = new Position2(bezier.CalculatePoint(t)).WithZ(startPoint.Z + zDiff * t);
            particles[i].Position = position;
        }

        transaction.Commit();
    }
}

