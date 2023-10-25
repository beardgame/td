using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDrawConnected")]
sealed class DrawConnected : ParticleUpdater<DrawConnected.IParameters>, IListener<DrawComponents>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Sprite { get; }
        Shader? Shader { get; }
        SpriteDrawGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }

        [Modifiable(1)]
        float UVLength { get; }

        bool AttachLastToObject { get; }
    }

    private readonly ParticleExtension<float> particleUVs;
    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public DrawConnected(IParameters parameters) : base(parameters)
    {
        particleUVs = new ParticleExtension<float>(onAddParticles);
    }

    public override void Activate()
    {
        base.Activate();

        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? SpriteDrawGroup.Particle, Parameters.DrawGroupOrderKey);

        Events.Subscribe(this);

        Particles.AddExtension(particleUVs);
    }

    private void onAddParticles(int index, int count)
    {
        var particles = Particles.ImmutableParticles;
        var uvs = particleUVs.MutableData;

        float u;
        Position3 previousPoint;

        if (index > 0)
        {
            previousPoint = particles[index - 1].Position;
            var currentPoint = particles[index].Position;
            var distance = (currentPoint - previousPoint).Length.NumericValue;
            u = uvs[index - 1] + distance / Parameters.UVLength;

            previousPoint = currentPoint;
        }
        else
        {
            previousPoint = particles[0].Position;
            u = StaticRandom.Float();
        }

        uvs[index] = u;

        for (var i = 1; i < count; i++)
        {
            var j = index + i;
            var currentPoint = particles[j].Position;
            var distance = (currentPoint - previousPoint).Length.NumericValue;
            u += distance / Parameters.UVLength;
            uvs[j] = u;
            previousPoint = currentPoint;
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Parameters.AttachLastToObject)
            attachLastToObject();
    }

    private void attachLastToObject()
    {
        if (Particles.Count < 2)
            return;

        var particles = Particles.MutableParticles;
        var uvs = particleUVs.MutableData;

        ref var lastParticlePosition = ref particles[^1].Position;
        var secondToLastParticlePosition = particles[^2].Position;

        lastParticlePosition = Owner.Position;
        var distance = (lastParticlePosition - secondToLastParticlePosition).Length.NumericValue;
        uvs[particles.Length - 1] = uvs[particles.Length - 2] + distance / Parameters.UVLength;
    }

    public void HandleEvent(DrawComponents e)
    {
        if (Particles.Count < 2)
            return;

        var particles = Particles.ImmutableParticles;

        e.Drawer.DrawIndexedVertices(sprite,
            vertexCount: particles.Length * 2,
            indexCount: (particles.Length - 1) * 6,
            out var vertices, out var indices, out var indexOffset, out var uv);

        var previous = particles[0];
        var current = particles[1];
        var offset = normalBetween(previous, current) * previous.Size;

        var uvs = particleUVs.ImmutableData;
        var u = uvs[0];
        vertices[0] = new UVColorVertex(previous.Position.NumericValue + offset.WithZ(), uv.Transform(new Vector2(u, 0)), previous.Color);
        vertices[1] = new UVColorVertex(previous.Position.NumericValue - offset.WithZ(), uv.Transform(new Vector2(u, 1)), previous.Color);

        for (var i = 2; i < particles.Length; i++)
        {
            var next = particles[i];
            offset = normalBetween(previous, next) * current.Size;

            u = uvs[i - 1];
            vertices[i * 2 - 2] = new UVColorVertex(current.Position.NumericValue + offset.WithZ(), uv.Transform(new Vector2(u, 0)), current.Color);
            vertices[i * 2 - 1] = new UVColorVertex(current.Position.NumericValue - offset.WithZ(), uv.Transform(new Vector2(u, 1)), current.Color);

            previous = current;
            current = next;
        }

        offset = normalBetween(previous, current) * current.Size;
        u = uvs[particles.Length - 1];
        vertices[^2] = new UVColorVertex(current.Position.NumericValue + offset.WithZ(), uv.Transform(new Vector2(u, 0)), current.Color);
        vertices[^1] = new UVColorVertex(current.Position.NumericValue - offset.WithZ(), uv.Transform(new Vector2(u, 1)), current.Color);

        var indexIndex = 0;
        for (var i = 0; i < particles.Length - 1; i++)
        {
            var vertexIndex = i * 2 + indexOffset;
            indices[indexIndex++] = (ushort)(vertexIndex + 0);
            indices[indexIndex++] = (ushort)(vertexIndex + 1);
            indices[indexIndex++] = (ushort)(vertexIndex + 2);
            indices[indexIndex++] = (ushort)(vertexIndex + 1);
            indices[indexIndex++] = (ushort)(vertexIndex + 3);
            indices[indexIndex++] = (ushort)(vertexIndex + 2);
        }
    }

    private static Vector2 normalBetween(in Particle p1, in Particle p2)
        => (p2.Position - p1.Position).NumericValue.Xy.PerpendicularRight.NormalizedSafe() * 0.5f;
}

