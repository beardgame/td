using System;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

internal interface IParticleExtension
{
    void Resize(int newCapacity);
    void Remove(Span<int> indices);
    void NotifyAdded(int index, int count);
}

sealed class ParticleExtension<T> : IParticleExtension
{
    private readonly Action<int, int> onAdded;

    private T[] data = Array.Empty<T>();
    private int count;

    public ReadOnlySpan<T> ImmutableData => data.AsSpan(0, count);
    public Span<T> MutableData => data.AsSpan(0, count);

    public ParticleExtension(Action<int, int> notifyAdded)
    {
        onAdded = notifyAdded;
    }

    void IParticleExtension.Resize(int newCapacity)
    {
        Array.Resize(ref data, newCapacity);
    }

    void IParticleExtension.Remove(Span<int> indices)
    {
        if (indices.Length == count)
        {
            count = 0;
            return;
        }

        var writeI = indices[0];
        var readI = writeI + 1;

        var nextRemoveI = 1;
        var nextReadIToSkip = indices.Length > nextRemoveI ? indices[nextRemoveI] : -1;

        for (; readI < count; readI++)
        {
            if (readI == nextReadIToSkip)
            {
                nextRemoveI++;
                nextReadIToSkip = indices.Length > nextRemoveI ? indices[nextRemoveI] : -1;
                continue;
            }
            data[writeI] = data[readI];
            writeI++;
        }

        count -= indices.Length;
    }

    void IParticleExtension.NotifyAdded(int index, int count)
    {
        this.count += count;
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

    void IParticleExtension.Resize(int newCapacity)
    {
    }

    void IParticleExtension.Remove(Span<int> indices)
    {
    }

    void IParticleExtension.NotifyAdded(int index, int count)
    {
        onAdded(index, count);
    }
}
