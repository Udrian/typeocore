using TypeOEngine.Typedeaf.Core.Collections;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>Maintains and updates registered updatable components.</summary>
        public class UpdateLoop
        {
            private DelayedList<IUpdatable> Updatables { get; set; }

            internal UpdateLoop()
            {
                Updatables = new DelayedList<IUpdatable>();
            }

            /// <summary>Updates each unpaused component and applies queued list changes.</summary>
            public void Update(double dt)
            {
                foreach(var updatable in Updatables)
                {
                    //if(entity.WillBeDeleted) continue; //TODO: Look over this

                    if(!updatable.Pause)
                    {
                        updatable.Update(dt);
                    }
                }

                Updatables.Process();
            }

            /// <summary>Queues an updatable component to be added to the loop.</summary>
            public void Push(IUpdatable updatable)
            {
                if(updatable == null) return;
                Updatables.Add(updatable);
            }

            /// <summary>Queues an updatable component to be removed from the loop.</summary>
            public void Pop(IUpdatable updatable)
            {
                if(updatable == null) return;
                Updatables.Remove(updatable);
            }
        }
    }
}
