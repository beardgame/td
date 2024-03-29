﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particles")]
sealed class Particles : Component<Particles.IParameters>
{
    public readonly struct AddTransaction
    {
        private readonly Particles particles;
        private readonly int index;
        private readonly int count;

        public AddTransaction(Particles particles, int index, int count)
        {
            this.particles = particles;
            this.index = index;
            this.count = count;
        }

        public void Commit()
        {
            foreach (var extension in particles.extensions)
                extension.NotifyAdded(index, count);
        }
    }

    public interface IParameters : IParametersTemplate<IParameters>
    {
        bool DontRemoveDeadParticles { get; }
    }

    private List<int>? indicesToRemove;
    private Particle[] particles = Array.Empty<Particle>();
    private int currentCount;

    public ReadOnlySpan<Particle> ImmutableParticles => particles.AsSpan(0, currentCount);
    public Span<Particle> MutableParticles => particles.AsSpan(0, currentCount);
    public int Count => currentCount;

    private readonly List<IParticleExtension> extensions = new();

    public Particles(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Parameters.DontRemoveDeadParticles)
            return;

        removeDeadParticles();
    }

    public void AddExtension(IParticleExtension extension)
    {
        extensions.Add(extension);
        if(particles.Length > 0)
            extension.Resize(particles.Length);
        if (currentCount > 0)
            extension.NotifyAdded(0, currentCount);
    }

    public void AddParticle(Particle particle)
    {
        ensureCapacity(currentCount + 1);
        particles[currentCount] = particle;
        currentCount++;

        foreach (var extension in extensions)
            extension.NotifyAdded(currentCount - 1, 1);
    }

    public Span<Particle> AddParticles(int count, out AddTransaction transaction)
    {
        ensureCapacity(currentCount + count);
        var span = particles.AsSpan(currentCount, count);
        transaction = new AddTransaction(this, currentCount, count);
        currentCount += count;
        return span;
    }

    private void ensureCapacity(int newCount)
    {
        if (newCount <= particles.Length)
            return;

        var newCapacity = Math.Max(10, Math.Max(newCount, particles.Length * 2));

        Array.Resize(ref particles, newCapacity);

        foreach (var extension in extensions)
            extension.Resize(newCapacity);
    }

    private void removeDeadParticles()
    {
        var now = Owner.Game.Time;
        var firstDeadCandidate = 0;

        for (; firstDeadCandidate < currentCount; firstDeadCandidate++)
        {
            if (!particles[firstDeadCandidate].IsAliveAtTime(now))
                break;
        }

        var allParticlesAlive = firstDeadCandidate == currentCount;
        if (allParticlesAlive)
            return;

        var newCount = firstDeadCandidate;

        if (extensions.Count == 0)
        {
            for (var i = firstDeadCandidate + 1; i < currentCount; i++)
            {
                if (particles[i].IsAliveAtTime(now))
                {
                    particles[newCount] = particles[i];
                    newCount++;
                }
            }
        }
        else
        {
            var removedIndices = indicesToRemove ??= new List<int>();
            removedIndices.Clear();
            removedIndices.Add(firstDeadCandidate);

            for (var i = firstDeadCandidate + 1; i < currentCount; i++)
            {
                if (particles[i].IsAliveAtTime(now))
                {
                    particles[newCount] = particles[i];
                    newCount++;
                }
                else
                {
                    removedIndices.Add(i);
                }
            }
            var removedIndicesSpan = CollectionsMarshal.AsSpan(removedIndices);

            foreach (var extension in extensions)
                extension.Remove(removedIndicesSpan);
        }

        currentCount = newCount;
    }
}

