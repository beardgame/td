using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static System.MathF;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("particleSystem")]
sealed class ParticleSystem<T> : Component<T, IParticleSystemParameters>, IDrawableComponent
    where T : IPositionable, IGameObject, IComponentOwner
{
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
        initializeParticles();
    }

    private void initializeSprite()
    {
        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader);
    }

    private void initializeParticles()
    {
        var vectorVelocity = Owner.TryGetSingleComponent<IDirected3>(out var directed)
            ? directed.Direction.NumericValue.NormalizedSafe() * Parameters.VectorVelocity
            : Velocity3.Zero;
        var position = Owner.Position;
        var now = Owner.Game.Time;
        for (var i = 0; i < particles.Length; i++)
        {
            var velocity = vectorVelocity + getRandomUnitVector3() * Parameters.RandomVelocity * StaticRandom.Float(0.5f, 1f);
            var timeOfDeath = now + Parameters.LifeTime * StaticRandom.Float(0.5f, 1f);
            particles[i] = new Particle(position, velocity, timeOfDeath);
        }
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

    public void Draw(IComponentDrawer drawer)
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
                    drawer.DrawSprite(
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

                    drawer.DrawQuad(sprite, p0, p1, p2, p3, argb);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}