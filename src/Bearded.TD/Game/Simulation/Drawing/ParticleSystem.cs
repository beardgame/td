using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Utilities.Vectors;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;



[Component("particleSystem")]
sealed class ParticleSystem : Component<ParticleSystem.IParameters>, IListener<DrawComponents>
{
    public enum DrawMode
    {
        Sprite,
        Line
    }

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        int Count { get; }
        ISpriteBlueprint Sprite { get; }
        Color Color { get; }
        Sprite.ColorMode ColorMode { get; }
        Shader? Shader { get; }
        SpriteDrawGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }

        Unit Size { get; }
        Unit? FinalSize { get; }
        Unit LineWidth { get; }
        TimeSpan LifeTime { get; }
        Velocity3 InherentVelocity { get; }
        Speed RandomVelocity { get; }
        Speed VectorVelocity { get; }
        Speed ReflectionVelocity { get; }
        [Modifiable(1)]
        float GravityFactor { get; }
        DrawMode DrawMode { get; }
        Difference3 Offset { get; }
        bool DontRandomize { get; }

        bool CollideWithLevel { get; }
        [Modifiable(1)]
        float CollisionNormalFactor { get; }
        [Modifiable(1)]
        float CollisionTangentFactor { get; }
    }

    private bool initialized;
    private readonly Particle[] particles;
    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public ParticleSystem(IParameters parameters) : base(parameters)
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

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();
        initializeSprite();
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    private void initializeSprite()
    {
        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? SpriteDrawGroup.Particle, Parameters.DrawGroupOrderKey);
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

        var baseVelocity = Parameters.InherentVelocity + reflectionVelocity + vectorVelocity;

        var position = Owner.Position + Parameters.Offset;
        var now = Owner.Game.Time;
        for (var i = 0; i < particles.Length; i++)
        {
            var velocity = baseVelocity + GetRandomUnitVector3() * Parameters.RandomVelocity * randomFloat(0.5f, 1);
            var timeOfDeath = now + Parameters.LifeTime * randomFloat(0.5f, 1);
            particles[i] = new Particle(position, velocity, timeOfDeath);
        }

        initialized = true;
    }

    private float randomFloat(float min, float max)
    {
        return Parameters.DontRandomize ? 1 : StaticRandom.Float(min, max);
    }


    public override void Update(TimeSpan elapsedTime)
    {
        if (!initialized)
            initializeParticles();

        var geometry = Owner.Game.GeometryLayer;

        var gravity = Constants.Game.Physics.Gravity3 * Parameters.GravityFactor;
        for (var i = 0; i < particles.Length; i++)
        {
            var particle = particles[i];
            var (p, v) = particle;

            if (Parameters.CollideWithLevel)
            {
                collideWithLevel(geometry, ref p, ref v, elapsedTime);
            }
            else
            {
                p += v * elapsedTime;
            }

            v += gravity * elapsedTime;

            particles[i] = particle.With(p, v);
        }
    }

    private void collideWithLevel(GeometryLayer geometry, ref Position3 p, ref Velocity3 v, TimeSpan elapsedTime)
    {
        var step = v * elapsedTime;

        var rayCaster = new LevelRayCaster();
        rayCaster.StartEnumeratingTiles(new Ray(p.XY(), step.XY()));

        var previousRayFactor = 0f;
        rayCaster.MoveNext(out var tile);

        while (true)
        {
            var info = geometry[tile];
            var tileType = info.Geometry.Type;

            var tileHeight = tileType switch {
                TileType.Floor => info.DrawInfo.Height,
                TileType.Wall => float.PositiveInfinity.U(),
                TileType.Crevice => float.NegativeInfinity.U(),
                _ => throw new ArgumentOutOfRangeException(),
            };

            var lastTile = !rayCaster.MoveNext(out tile);
            var rayFactor = lastTile ? 1 : rayCaster.CurrentRayFactor;

            var floorCollisionFactor = (tileHeight - p.Z) / step.Z;
            if (floorCollisionFactor > previousRayFactor && floorCollisionFactor <= rayFactor)
            {
                reflect(ref p, ref v, p + step * floorCollisionFactor, Vector3.UnitZ);
                return;
            }

            if (lastTile)
                break;

            var heightAtTileBorder = p.Z + step.Z * rayFactor;

            if (heightAtTileBorder < tileHeight)
            {
                reflect(ref p, ref v, p + step * rayFactor, (-rayCaster.LastStep!.Value.Vector()).WithZ());
                return;
            }

            previousRayFactor = rayFactor;
        }

        p += step;
    }

    private void reflect(ref Position3 p, ref Velocity3 v, Position3 pointOfReflection, Vector3 normal)
    {
        normal = normal.NormalizedSafe();

        var dotWithVelocityOutMagnitude = Vector3.Dot(normal, -v.NumericValue);

        var normalVelocityOut = new Velocity3(normal * dotWithVelocityOutMagnitude);
        var tangentVelocity = v + normalVelocityOut;

        var velocityOut = normalVelocityOut * Parameters.CollisionNormalFactor + tangentVelocity * Parameters.CollisionNormalFactor;

        v = velocityOut;
        p = pointOfReflection + normal * 0.01.U();
    }

    public void HandleEvent(DrawComponents e)
    {
        var now = Owner.Game.Time;

        var color = Sprite.GetColor(Owner, Parameters.ColorMode, Parameters.Color);

        foreach (var p in particles)
        {
            var a = (float)((p.TimeOfDeath - now) / (Parameters.LifeTime * 0.5));
            if (a <= 0)
                continue;
            a = Math.Min(a, 1);
            var argb = color * a;
            var size = Parameters.FinalSize.HasValue
                ? Interpolate.Lerp(Parameters.FinalSize.Value.NumericValue, Parameters.Size.NumericValue, (float)((p.TimeOfDeath - now) / Parameters.LifeTime))
                : Parameters.Size.NumericValue;

            switch (Parameters.DrawMode)
            {
                case DrawMode.Sprite:
                    e.Drawer.DrawSprite(
                        sprite,
                        p.Position.NumericValue,
                        size,
                        p.Velocity.XY().Direction.Radians,
                        argb);
                    break;
                case DrawMode.Line:

                    var v = p.Velocity.NumericValue * size * 0.5f;
                    var w = Vector3.Cross(v.NormalizedSafe(), Vector3.UnitZ) * Parameters.LineWidth.NumericValue * 0.5f;
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
