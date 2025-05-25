using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators;

static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(
        this IncrementalValuesProvider<TSource?> source)
        where TSource : class
    {
        return source.SelectMany(static (item, _) =>
            item == null
                ? ImmutableArray<TSource>.Empty
                : ImmutableArray.Create(item)
        );
    }
    public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(
        this IncrementalValuesProvider<TSource?> source)
        where TSource : struct
    {
        return source.SelectMany(static (item, _) =>
            item == null
                ? ImmutableArray<TSource>.Empty
                : ImmutableArray.Create((TSource)item)
        );
    }
}
