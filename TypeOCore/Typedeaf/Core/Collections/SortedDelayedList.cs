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
                var index = BinarySearch(item);
                if(index < 0) index = ~index;
                BaseInsert(index, item);
            }
        }
    }
}
