namespace TypeOEngine.Typedeaf.Core
{
    namespace Collections
    {
        public class SortedDelayedList<T> : DelayedList<T>
        {
            protected override void ProcessAdd(T item)
            {
                var index = BinarySearch(item);
                if(index < 0) index = ~index;
                BaseInsert(index, item);
            }
        }
    }
}
