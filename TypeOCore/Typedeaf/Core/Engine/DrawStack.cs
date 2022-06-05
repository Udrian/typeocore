using TypeOEngine.Typedeaf.Core.Collections;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public class DrawStack
        {
            private SortedDelayedList<IDrawable> Drawables { get; set; }

            internal DrawStack()
            {
                Drawables = new SortedDelayedList<IDrawable>();
            }

            public void Draw(Canvas canvas)
            {
                var needSort = false;
                var lastDraworder = int.MinValue;
                foreach(var drawable in Drawables)
                {
                    if(!needSort && lastDraworder > drawable.DrawOrder)
                    {
                        needSort = true;
                    }
                    lastDraworder = drawable.DrawOrder;
                    //if(entity.WillBeDeleted) continue; //TODO: Look over this
                    if(drawable.Hidden) continue;
                    drawable.Draw(canvas);
                }
                if(needSort) //TODO: A problem with this solution is that the draw order won't come in effect until next tick
                {
                    Drawables.Sort();
                }

                /*foreach(var entity in HasEntities) //TODO: Don't know yet if we want nested Drawstacks, maybe? look into it
                {
                    if((entity as Entity)?.WillBeDeleted == true) continue;
                    if((entity as IDrawable)?.Hidden == true) continue;
                    entity.Entities.Draw(canvas);
                }*/

                Drawables.Process();
            }

            public void Push(IDrawable drawable)
            {
                if(drawable == null) return;
                Drawables.Add(drawable);
            }

            public void Pop(IDrawable drawable)
            {
                if(drawable == null) return;
                Drawables.Remove(drawable);
            }
        }
    }
}
