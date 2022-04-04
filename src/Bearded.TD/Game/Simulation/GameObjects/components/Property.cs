using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IProperty<out T>
{
    T Value { get; }
}

sealed class Property<TValue> : Component, IProperty<TValue>
{
    public TValue Value { get; }

    public Property(TValue value)
    {
        Value = value;
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
