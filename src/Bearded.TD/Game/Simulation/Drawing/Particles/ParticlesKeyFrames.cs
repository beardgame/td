using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing.Animation;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesKeyFrames")]
sealed class ParticlesKeyFrames : ParticleUpdater<ParticlesKeyFrames.IParameters>
{
    internal readonly record struct KeyFrame(
        TimeSpan Duration,
        float Size,
        Color Color
        ) : IKeyFrame<KeyFrame>
    {
        private static readonly IInterpolationMethod1d interpolation = Interpolation1d.Linear;

        public KeyFrame InterpolateTowards(KeyFrame other, double t)
        {
            var size = (float)interpolation.Interpolate(Size, other.Size, t);
            var color = Color.Lerp(Color, other.Color, (float)t);

            return new KeyFrame(
                default,
                size,
                color
            );
        }
    }

    public interface IParameters : IParametersTemplate<IParameters>
    {
        bool ScaleToParticleLifeTime { get; }
        IKeyFrameAnimation<KeyFrame> Animation { get; }
    }

    public ParticlesKeyFrames(IParameters parameters) : base(parameters)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var now = Owner.Game.Time;
        var duration = Parameters.ScaleToParticleLifeTime
            ? Parameters.Animation.TotalDuration
            : TimeSpan.One;

        foreach (ref var p in Particles.MutableParticles)
        {
            var t = duration * p.AgeFactorAtTime(now);
            var keyFrame = Parameters.Animation.InterpolateFrameAt(t);

            p.Size = keyFrame.Size;
            p.Color = keyFrame.Color;
        }
    }
}

