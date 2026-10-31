using System.Collections.Generic;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Collections
    {
        /// <summary>A list that queues additions and removals until its changes are processed.</summary>
        public class DelayedList<T> : List<T>
        {
            private Queue<T> AddQueue { get; set; }
            private Queue<T> RemoveQueue { get; set; }
            private bool ToBeCleared { get; set; }

            /// <summary>Initializes an empty delayed list.</summary>
            public DelayedList()
            {
                AddQueue = new Queue<T>();
                RemoveQueue = new Queue<T>();
                ToBeCleared = false;
            }

            /// <summary>Inserts an item at the specified index; this operation is not implemented.</summary>
            public new void Insert(int index, T item)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>Queues the item at the specified index for removal.</summary>
            public new void RemoveAt(int index)
            {
                RemoveQueue.Enqueue(this[index]);
            }

            /// <summary>Queues an item to be added when the list is processed.</summary>
            public new void Add(T item)
            {
                AddQueue.Enqueue(item);
            }

            /// <summary>Queues all current items to be cleared when the list is processed.</summary>
            public new void Clear()
            {
                ToBeCleared = true;
            }

            /// <summary>Queues an existing item for removal and reports whether it is present.</summary>
            public new bool Remove(T item)
            {
                if(Contains(item))
                {
                    RemoveQueue.Enqueue(item);
                    return true;
                }

                return false;
            }

            /// <summary>Applies queued additions and removals to the underlying list.</summary>
            public void Process()
            {
                if(ToBeCleared)
                {
                    base.Clear();
                    ToBeCleared = false;
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

            /// <summary>Processes a queued addition; derived lists can override its insertion behavior.</summary>
            protected virtual void ProcessAdd(T item)
            {
                BaseAdd(item);
            }

            /// <summary>Adds an item directly to the underlying list.</summary>
            protected void BaseAdd(T item)
            {
                base.Add(item);
            }

            /// <summary>Inserts an item directly into the underlying list at the specified index.</summary>
            protected void BaseInsert(int index, T item)
            {
                base.Insert(index, item);
            }
        }
    }
}
