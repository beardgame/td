using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static System.MathF;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("particleSystem")]
sealed class ParticleSystem : Component<IParticleSystemParameters>, IListener<DrawComponents>
{
    private bool initialized;
    private readonly Particle[] particles;
    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public ParticleSystem(IParticleSystemParameters parameters) : base(parameters)
    {
        particles = new Particle[parameters.Count];
    }

    private readonly struct Particle
    {
        public Position3 Position { get; }
        public Velocity3 Velocity { get; }
        public Instant TimeOfDeath { get; }

        public Particle(Position3 position, Velocity3 velocity, Instant timeOfDeath)
        {
            Position = position;
            Velocity = velocity;
            TimeOfDeath = timeOfDeath;
        }

        public Particle With(Position3 position, Velocity3 velocity)
            => new(position, velocity, TimeOfDeath);

        public void Deconstruct(out Position3 p, out Velocity3 v)
        {
            p = Position;
            v = Velocity;
        }
    }

    protected override void OnAdded()
    {
        initializeSprite();
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    private void initializeSprite()
    {
        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader);
    }

    private void initializeParticles()
    {
        var hitInfo = Owner.TryGetSingleComponent<IProperty<HitInfo>>(out var h) ? h : null;

        var reflectionVelocity = hitInfo != null
            ? hitInfo.Value.GetReflection().NumericValue * Parameters.ReflectionVelocity
            : Velocity3.Zero;

        var vectorVelocity = Owner.TryGetSingleComponent<IDirected3>(out var directed)
            ? directed.Direction.NumericValue.NormalizedSafe() * Parameters.VectorVelocity
            : Velocity3.Zero;

        var baseVelocity = reflectionVelocity + vectorVelocity;

        var position = Owner.Position;
        var now = Owner.Game.Time;
        for (var i = 0; i < particles.Length; i++)
        {
            var velocity = baseVelocity + getRandomUnitVector3() * Parameters.RandomVelocity * StaticRandom.Float(0.5f, 1f);
            var timeOfDeath = now + Parameters.LifeTime * StaticRandom.Float(0.5f, 1f);
            particles[i] = new Particle(position, velocity, timeOfDeath);
        }

        initialized = true;
    }

    private static Vector3 getRandomUnitVector3()
    {
        var theta = Tau * StaticRandom.Float();
        var phi = Acos(1 - 2 * StaticRandom.Float());
        var sinPhi = Sin(phi);
        return new Vector3(
            sinPhi * Cos(theta),
            sinPhi * Sin(theta),
            Cos(phi)
        );
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (!initialized)
            initializeParticles();

        var gravity = Constants.Game.Physics.Gravity3;
        for (var i = 0; i < particles.Length; i++)
        {
            var particle = particles[i];
            var (p, v) = particle;

            p += v * elapsedTime;
            v += gravity * elapsedTime;

            particles[i] = particle.With(p, v);
        }
    }

    public void HandleEvent(DrawComponents e)
    {
        var now = Owner.Game.Time;
        foreach (var p in particles)
        {
            var a = (float)((p.TimeOfDeath - now) / (Parameters.LifeTime * 0.5));
            if (a <= 0)
                continue;
            a = Math.Min(a, 1);
            var argb = Parameters.Color.WithAlpha(0) * a;

            switch (Parameters.DrawMode)
            {
                case ParticleDrawMode.Sprite:
                    e.Drawer.DrawSprite(
                        sprite,
                        p.Position.NumericValue,
                        Parameters.Size,
                        p.Velocity.XY().Direction.Radians,
                        argb);
                    break;
                case ParticleDrawMode.Line:

                    var v = p.Velocity.NumericValue * Parameters.Size * 0.5f;
                    var w = Vector3.Cross(v.NormalizedSafe(), Vector3.UnitZ) * Parameters.LineWidth * 0.5f;
                    var c = p.Position.NumericValue;

                    var p0 = c - v + w;
                    var p1 = c + v + w;
                    var p2 = c + v - w;
                    var p3 = c - v - w;

                    e.Drawer.DrawQuad(sprite, p0, p1, p2, p3, argb);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
