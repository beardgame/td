using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Utilities;

sealed class Binding<T> : IReadonlyBinding<T>
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
        if (value?.Equals(Value) ?? Value is null) return;
        Value = value;
        ControlUpdated?.Invoke(value);
    }

    public void SetFromSource(T value)
    {
        if (value?.Equals(Value) ?? Value is null) return;
        Value = value;
        SourceUpdated?.Invoke(value);
    }
}

interface IReadonlyBinding<out T>
{
    T Value { get; }
    event GenericEventHandler<T>? ControlUpdated;
    event GenericEventHandler<T>? SourceUpdated;
}

static class Binding
{
    public static T ReturnCurrentAndKeepUpdated<T>(this IReadonlyBinding<T> binding, Action<T> update)
    {
        binding.SourceUpdated += t => update(t);
        return binding.Value;
    }

    public static IReadonlyBinding<T> Constant<T>(T value) => Create(value);

    public static Binding<T> Create<T>(T initialValue) => new(initialValue);

    public static Binding<T> Create<T>(T initialValue, GenericEventHandler<T> syncBack)
    {
        var binding = new Binding<T>(initialValue);
        binding.ControlUpdated += syncBack;
        return binding;
    }

    public static IReadonlyBinding<bool> And(this IReadonlyBinding<bool> left, IReadonlyBinding<bool> right) =>
        Combine(left, right, (l, r) => l && r);

    public static IReadonlyBinding<bool> Or(this IReadonlyBinding<bool> left, IReadonlyBinding<bool> right) =>
        Combine(left, right, (l, r) => l || r);

    public static IReadonlyBinding<bool> Negate(this IReadonlyBinding<bool> binding) => binding.Transform(b => !b);

    public static IReadonlyBinding<TOut> Transform<TIn, TOut>(this IReadonlyBinding<TIn> binding, Func<TIn, TOut> func)
    {
        var transformed = new Binding<TOut>(func(binding.Value));
        binding.ControlUpdated += v => transformed.SetFromControl(func(v));
        binding.SourceUpdated += v => transformed.SetFromSource(func(v));
        return transformed;
    }

    public static IReadonlyBinding<TOut> Combine<TLeft, TRight, TOut>(
        IReadonlyBinding<TLeft> left, IReadonlyBinding<TRight> right, Func<TLeft, TRight, TOut> combine)
    {
        var combined = new Binding<TOut>(combine(left.Value, right.Value));
        left.ControlUpdated += v => combined.SetFromControl(combine(v, right.Value));
        right.ControlUpdated += v => combined.SetFromControl(combine(left.Value, v));
        left.SourceUpdated += v => combined.SetFromSource(combine(v, right.Value));
        right.SourceUpdated += v => combined.SetFromSource(combine(left.Value, v));
        return combined;
    }

    public static IReadonlyBinding<TOut> Aggregate<TIn, TOut>(
        IEnumerable<IReadonlyBinding<TIn>> bindings, Func<IEnumerable<TIn>, TOut> aggregator)
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

    public static IReadonlyBinding<Void> AggregateChanges<TIn>(IEnumerable<Binding<TIn>> bindings)
    {
        var aggregated = new Binding<Void>();
        foreach (var binding in bindings)
        {
            binding.ControlUpdated += _ => aggregated.SetFromControl(default);
            binding.SourceUpdated += _ => aggregated.SetFromSource(default);
        }

        return aggregated;
    }

    public static IReadonlyBinding<int> CollectionSize<TCollection, TElement>(this Binding<TCollection> collection)
        where TCollection : ICollection<TElement>
    {
        var count = new Binding<int>(collection.Value.Count);
        collection.ControlUpdated += newColl => count.SetFromControl(newColl.Count);
        collection.SourceUpdated += newColl => count.SetFromSource(newColl.Count);
        return count;
    }

    public static IReadonlyBinding<TElement?> ListElementByIndex<TList, TElement>(this Binding<TList> list, int index)
        where TList : IList<TElement>
    {
        var element = new Binding<TElement?>(elementOrDefault(list.Value));
        list.ControlUpdated += newList => element.SetFromControl(elementOrDefault(newList));
        list.SourceUpdated += newList => element.SetFromSource(elementOrDefault(newList));
        return element;

        TElement? elementOrDefault(TList l) => index < l.Count ? l[index] : default;
    }
}
