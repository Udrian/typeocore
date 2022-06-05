using System.Collections.Generic;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Collections
    {
        public class DelayedList<T> : List<T>
        {
            private Queue<T> AddQueue { get; set; }
            private Queue<T> RemoveQueue { get; set; }
            private bool ToBeCleared { get; set; }

            public DelayedList()
            {
                AddQueue = new Queue<T>();
                RemoveQueue = new Queue<T>();
                ToBeCleared = false;
            }

            public new void Insert(int index, T item)
            {
                throw new System.NotImplementedException();
            }

            public new void RemoveAt(int index)
            {
                RemoveQueue.Enqueue(this[index]);
            }

            public new void Add(T item)
            {
                AddQueue.Enqueue(item);
            }

            public new void Clear()
            {
                ToBeCleared = true;
            }

            public new bool Remove(T item)
            {
                if(Contains(item))
                {
                    RemoveQueue.Enqueue(item);
                    return true;
                }

                return false;
            }

            public void Process()
            {
                if(ToBeCleared)
                {
                    Clear();
                    AddQueue.Clear();
                    RemoveQueue.Clear();
                    return;
                }

                while(AddQueue.Count > 0)
                {
                    ProcessAdd(AddQueue.Dequeue());
                }
                while(RemoveQueue.Count > 0)
                {
                    base.Remove(RemoveQueue.Dequeue());
                }

                return;
            }

            protected virtual void ProcessAdd(T item)
            {
                BaseAdd(item);
            }

            protected void BaseAdd(T item)
            {
                base.Add(item);
            }

            protected void BaseInsert(int index, T item)
            {
                base.Insert(index, item);
            }
        }
    }
}
