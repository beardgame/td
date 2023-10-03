using System;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

internal interface IParticleExtension
{
    void Resize(int newSize);
    void Move(int oldIndex, int newIndex);
    void NotifyAdded(int index, int count);
}

sealed class ParticleExtension<T> : IParticleExtension
{
    private T[] data = Array.Empty<T>();
    private readonly Action<int, int> onAdded;

    public ReadOnlySpan<T> ImmutableData => data;
    public Span<T> MutableData => data;

    public ParticleExtension(Action<int, int> notifyAdded)
    {
        onAdded = notifyAdded;
    }

    void IParticleExtension.Resize(int newSize)
    {
        Array.Resize(ref data, newSize);
    }

    void IParticleExtension.Move(int oldIndex, int newIndex)
    {
        data[newIndex] = data[oldIndex];
    }

    void IParticleExtension.NotifyAdded(int index, int count)
    {
        onAdded(index, count);
    }
}

sealed class NotificationOnlyParticleExtension : IParticleExtension
{
    private readonly Action<int, int> onAdded;

    public NotificationOnlyParticleExtension(Action<int, int> notifyAdded)
    {
        onAdded = notifyAdded;
    }

    void IParticleExtension.Resize(int newSize)
    {
    }

    void IParticleExtension.Move(int oldIndex, int newIndex)
    {
    }

    void IParticleExtension.NotifyAdded(int index, int count)
    {
        onAdded(index, count);
    }
}
