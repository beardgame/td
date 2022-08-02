using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities;

namespace Bearded.TD.Utilities;

sealed class Binding<T>
{
    public T Value { get; private set; }

    public event GenericEventHandler<T>? ControlUpdated;
    public event GenericEventHandler<T>? SourceUpdated;

    public Binding() : this(default) { }

    public Binding(T initialValue)
    {
        Value = initialValue;
    }

    public void SetFromControl(T value)
    {
        if (value.Equals(Value)) return;
        Value = value;
        ControlUpdated?.Invoke(value);
    }

    public void SetFromSource(T value)
    {
        if (value.Equals(Value)) return;
        Value = value;
        SourceUpdated?.Invoke(value);
    }
}

static class Binding
{
    public static Binding<T> Create<T>(T initialValue) => new(initialValue);

    public static Binding<T> Create<T>(T initialValue, GenericEventHandler<T> syncBack)
    {
        var binding = new Binding<T>(initialValue);
        binding.ControlUpdated += syncBack;
        return binding;
    }

    public static Binding<bool> And(this Binding<bool> left, Binding<bool> right) =>
        Combine(left, right, (l, r) => l && r);

    public static Binding<bool> Or(this Binding<bool> left, Binding<bool> right) =>
        Combine(left, right, (l, r) => l || r);

    public static Binding<bool> Negate(this Binding<bool> binding) => binding.Transform(b => !b);

    public static Binding<TOut> Transform<TIn, TOut>(this Binding<TIn> binding, Func<TIn, TOut> func)
    {
        var transformed = new Binding<TOut>(func(binding.Value));
        binding.ControlUpdated += v => transformed.SetFromControl(func(v));
        binding.SourceUpdated += v => transformed.SetFromSource(func(v));
        return transformed;
    }

    public static Binding<TOut> Combine<TLeft, TRight, TOut>(
        Binding<TLeft> left, Binding<TRight> right, Func<TLeft, TRight, TOut> combine)
    {
        var combined = new Binding<TOut>(combine(left.Value, right.Value));
        left.ControlUpdated += v => combined.SetFromControl(combine(v, right.Value));
        right.ControlUpdated += v => combined.SetFromControl(combine(left.Value, v));
        left.SourceUpdated += v => combined.SetFromSource(combine(v, right.Value));
        right.SourceUpdated += v => combined.SetFromSource(combine(left.Value, v));
        return combined;
    }

    public static Binding<TOut> Aggregate<TIn, TOut>(
        IEnumerable<Binding<TIn>> bindings, Func<IEnumerable<TIn>, TOut> aggregator)
    {
        var bindingsArray = bindings.ToImmutableArray();
        var aggregated = new Binding<TOut>(aggregatedValue());
        foreach (var binding in bindingsArray)
        {
            binding.ControlUpdated += _ => aggregated.SetFromControl(aggregatedValue());
            binding.SourceUpdated += _ => aggregated.SetFromSource(aggregatedValue());
        }

        return aggregated;

        TOut aggregatedValue() => aggregator(bindingsArray.Select(b => b.Value));
    }
}
