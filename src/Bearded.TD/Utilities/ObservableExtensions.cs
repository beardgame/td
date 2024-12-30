using System;
using System.Reactive.Linq;

namespace Bearded.TD.Utilities;

static class ObservableExtensions
{
    public static IObservable<(T Previous, T Current)> LastTwo<T>(this IObservable<T> source)
    {
        return source
            .Scan(
                default((T, T Current)),
                (previous, current) => (previous.Current, current)
            )
            .Skip(1);
    }

    public static (IObservable<T> WhereTrue, IObservable<T> WhereFalse) Split<T>(this IObservable<T> source, Func<T, bool> predicate)
    {
        return (
            source.Where(predicate),
            source.Where(v => !predicate(v))
        );
    }
}
