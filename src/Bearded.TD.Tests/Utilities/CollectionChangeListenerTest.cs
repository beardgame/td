using System.Collections.ObjectModel;
using Bearded.TD.Utilities;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace Bearded.TD.Tests.Utilities;

[TestSubject(typeof(CollectionChangeListener))]
public sealed class CollectionChangeListenerTest
{
    private readonly ObservableCollection<string> collection = ["apple", "banana", "coconut"];

    [Fact]
    public void InsertNotifiesHandler()
    {
        var handler = new Handler();
        var listener = collection.ConsumeChanges(handler);

        collection.Insert(1, "strawberry");

        handler.LastAddedCall.Should().Be(("strawberry", 1));

        listener.Dispose();
    }

    [Fact]
    public void AddNotifiesHandler()
    {
        var handler = new Handler();
        var listener = collection.ConsumeChanges(handler);

        var expectedIndex = collection.Count;
        collection.Add("strawberry");

        handler.LastAddedCall.Should().Be(("strawberry", expectedIndex));

        listener.Dispose();
    }

    [Fact]
    public void RemoveAtNotifiesHandler()
    {
        var handler = new Handler();
        var listener = collection.ConsumeChanges(handler);

        var removedItem = collection[1];
        collection.RemoveAt(1);

        handler.LastRemovedCall.Should().Be((removedItem, 1));

        listener.Dispose();
    }

    [Fact]
    public void RemoveNotifiesHandler()
    {
        var handler = new Handler();
        var listener = collection.ConsumeChanges(handler);

        var expectedIndex = collection.IndexOf("apple");
        collection.Remove("apple");

        handler.LastRemovedCall.Should().Be(("apple", expectedIndex));

        listener.Dispose();
    }

    [Fact]
    public void SetByIndexNotifiesHandler()
    {
        var handler = new Handler();
        var listener = collection.ConsumeChanges(handler);

        var oldItem = collection[2];
        collection[2] = "strawberry";

        handler.LastReplacedCall.Should().Be((oldItem, "strawberry", 2));

        listener.Dispose();
    }

    [Fact]
    public void MoveNotifiesHandler()
    {
        var handler = new Handler();
        var listener = collection.ConsumeChanges(handler);

        var movedItem = collection[1];
        collection.Move(1, 2);

        handler.LastMovedCall.Should().Be((movedItem, 1, 2));

        listener.Dispose();
    }

    [Fact]
    public void ClearNotifiesHandler()
    {
        var handler = new Handler();
        var listener = collection.ConsumeChanges(handler);

        collection.Clear();

        handler.ResetCalled.Should().BeTrue();

        listener.Dispose();
    }

    private sealed class Handler : ICollectionChangeHandler<string>
    {
        public (string Item, int Index)? LastAddedCall { get; private set; }
        public (string Item, int Index)? LastRemovedCall { get; private set; }
        public (string OldItem, string NewItem, int Index)? LastReplacedCall { get; private set; }
        public (string Item, int OldIndex, int NewIndex)? LastMovedCall { get; private set; }
        public bool ResetCalled { get; private set; }

        public void OnItemAdded(string item, int index)
        {
            LastAddedCall = (item, index);
        }

        public void OnItemRemoved(string item, int index)
        {
            LastRemovedCall = (item, index);
        }

        public void OnItemReplaced(string oldItem, string newItem, int index)
        {
            LastReplacedCall = (oldItem, newItem, index);
        }

        public void OnItemMoved(string item, int oldIndex, int newIndex)
        {
            LastMovedCall = (item, oldIndex, newIndex);
        }

        public void OnReset()
        {
            ResetCalled = true;
        }
    }
}
