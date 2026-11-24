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

    [Fact]
    public void ProcessRemovesRepeatedItemsWithoutRemovingUnrequestedDuplicates()
    {
        var list = new DelayedList<int> { 1, 2, 1, 3, 1 };
        list.Process();

        Assert.True(list.Remove(1));
        Assert.True(list.Remove(1));
        list.Process();

        Assert.Equal(new[] { 2, 3, 1 }, list);
    }

    [Fact]
    public void SortedDelayedListAppendsLargeAlreadySortedBatchesInLinearComparisons()
    {
        const int itemCount = 20_000;
        var list = new SortedDelayedList<CountingComparable>();
        CountingComparable.ComparisonCount = 0;

        for(var i = 0; i < itemCount; i++)
            list.Add(new CountingComparable(i));

        list.Process();

        Assert.Equal(itemCount, list.Count);
        Assert.InRange(CountingComparable.ComparisonCount, 0, itemCount);
    }

    private sealed class CountingComparable : IComparable<CountingComparable>
    {
        public static int ComparisonCount { get; set; }
        private readonly int _value;

        public CountingComparable(int value)
        {
            _value = value;
        }

        public int CompareTo(CountingComparable other)
        {
            ComparisonCount++;
            return _value.CompareTo(other._value);
        }
    }
}
