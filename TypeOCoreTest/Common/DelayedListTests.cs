using TypeOEngine.Typedeaf.Core.Collections;

namespace TypeOCoreTest.Common;

public class DelayedListTests
{
    [Fact]
    public void AddAndRemoveAreAppliedWhenProcessed()
    {
        var list = new DelayedList<string>();
        list.Add("first");
        list.Add("second");

        Assert.Empty(list);
        list.Process();
        Assert.Equal(new[] { "first", "second" }, list);

        Assert.True(list.Remove("first"));
        Assert.False(list.Remove("missing"));
        Assert.Contains("first", list);
        list.Process();
        Assert.Equal(new[] { "second" }, list);
    }

    [Fact]
    public void ClearRemovesCurrentAndQueuedItemsAndDoesNotRemainLatched()
    {
        var list = new DelayedList<int> { 1 };
        list.Process();
        list.Add(2);
        list.Clear();

        list.Process();
        Assert.Empty(list);

        list.Add(3);
        list.Process();
        Assert.Equal(new[] { 3 }, list);
    }

    [Fact]
    public void RemoveAtIsDeferred()
    {
        var list = new DelayedList<int> { 10, 20 };
        list.Process();
        list.RemoveAt(0);

        Assert.Equal(new[] { 10, 20 }, list);
        list.Process();
        Assert.Equal(new[] { 20 }, list);
    }

    [Fact]
    public void SortedDelayedListInsertsItemsInOrder()
    {
        var list = new SortedDelayedList<int>();
        list.Add(5);
        list.Add(1);
        list.Add(3);

        list.Process();
        Assert.Equal(new[] { 1, 3, 5 }, list);
    }
}
