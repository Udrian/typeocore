namespace TypeOEngine.Typedeaf.Core
{
    namespace Collections
    {
        /// <summary>A delayed list that inserts queued items in sorted order.</summary>
        public class SortedDelayedList<T> : DelayedList<T>
        {
            /// <summary>Inserts a queued item at its sorted position.</summary>
            protected override void ProcessAdd(T item)
            {
                if(Count == 0 || Comparer<T>.Default.Compare(this[Count - 1], item) <= 0)
                {
                    BaseAdd(item);
                    return;
                }

                var index = BinarySearch(item);
                if(index < 0) index = ~index;
                BaseInsert(index, item);
            }
        }
    }
}
