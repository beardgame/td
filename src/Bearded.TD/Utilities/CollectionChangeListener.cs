using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Bearded.TD.Utilities;

static class CollectionChangeListener
{
    public static IDisposable ConsumeChanges<T>(
        this INotifyCollectionChanged notifier, ICollectionChangeHandler<T> handler)
    {
        return new Implementation<T>(notifier, handler);
    }

    private sealed class Implementation<T> : IDisposable
    {
        private readonly INotifyCollectionChanged notifier;
        private readonly ICollectionChangeHandler<T> handler;

        public Implementation(INotifyCollectionChanged notifier, ICollectionChangeHandler<T> handler)
        {
            this.notifier = notifier;
            this.handler = handler;

            this.notifier.CollectionChanged += onCollectionChanged;
        }

        public void Dispose()
        {
            notifier.CollectionChanged -= onCollectionChanged;
        }

        private void onCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    assertSingleItemList(e.NewItems);
                    assertValidIndex(e.NewStartingIndex);
                    handler.OnItemAdded(onlyItem(e.NewItems), e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    assertSingleItemList(e.OldItems);
                    assertValidIndex(e.OldStartingIndex);
                    handler.OnItemRemoved(onlyItem(e.OldItems), e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    assertSingleItemList(e.NewItems);
                    assertSingleItemList(e.OldItems);
                    assertValidIndex(e.NewStartingIndex);
                    handler.OnItemReplaced(onlyItem(e.OldItems), onlyItem(e.NewItems), e.NewStartingIndex);

                    break;

                case NotifyCollectionChangedAction.Move:
                    assertSingleItemList(e.NewItems);
                    assertValidIndex(e.NewStartingIndex);
                    assertValidIndex(e.OldStartingIndex);
                    handler.OnItemMoved(onlyItem(e.NewItems), e.OldStartingIndex, e.NewStartingIndex);

                    break;

                case NotifyCollectionChangedAction.Reset:
                    handler.OnReset();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        // This [NotNull] annotation may be confusing, but essentially tells the compiler this value is not null past
        // the invocation of this method. Since this method throws an exception otherwise, this is a safe statement to
        // make.
        private static void assertSingleItemList([NotNull] IList? list)
        {
            ArgumentNullException.ThrowIfNull(list);
            if (list.Count != 1)
            {
                throw new ArgumentException("Expected a single item list", nameof(list));
            }
            ArgumentNullException.ThrowIfNull(list[0]);
        }

        private static void assertValidIndex(int index)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        }

        private static T onlyItem(IList list) => (T) list[0]!;
    }
}

interface ICollectionChangeHandler<in T>
{
    public void OnItemAdded(T item, int index);
    public void OnItemRemoved(T item, int index);
    public void OnItemReplaced(T oldItem, T newItem, int index);
    public void OnItemMoved(T item, int oldIndex, int newIndex);
    public void OnReset();
}
