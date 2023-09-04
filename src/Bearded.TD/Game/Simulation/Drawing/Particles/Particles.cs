using System;
using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particles")]
sealed class Particles : Component
{
    private Particle[] particles = Array.Empty<Particle>();
    private int currentCount;

    public ReadOnlySpan<Particle> ImmutableParticles => particles.AsSpan(0, currentCount);
    public Span<Particle> MutableParticles => particles.AsSpan(0, currentCount);
    public int Count => currentCount;

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        removeDeadParticles();
    }
    public void AddParticle(Particle particle)
    {
        ensureCapacity(currentCount + 1);
        particles[currentCount] = particle;
        currentCount++;
    }

    public Span<Particle> AddParticles(int count)
    {
        ensureCapacity(currentCount + count);
        var span = particles.AsSpan(currentCount, count);
        currentCount += count;
        return span;
    }

    private void ensureCapacity(int newCapacity)
    {
        if (newCapacity <= particles.Length)
            return;

        var newCount = Math.Max(10, Math.Max(newCapacity, particles.Length * 2));

        var newParticles = new Particle[newCount];
        particles.CopyTo(newParticles, 0);
        particles = newParticles;
    }

    private void removeDeadParticles()
    {
        var now = Owner.Game.Time;
        var newCount = 0;
        for (var i = 0; i < currentCount; i++)
        {
            if (particles[i].IsAliveAtTime(now))
            {
                particles[newCount] = particles[i];
                newCount++;
            }
        }

        currentCount = newCount;
    }
}

