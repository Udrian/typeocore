using System;
using System.Collections.Generic;
using System.Linq;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public class DrawableManager<T> : IHasContext where T : Drawable
        {
            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            internal List<T> Drawables { get; private set; }
            private DrawStack DrawStack { get; set; }
            private object Parent { get; set; }

            internal DrawableManager(DrawStack drawStack, object parent)
            {
                Drawables = new List<T>();
                DrawStack = drawStack;
                Parent = parent;
            }

            public Drawable Create(Type type, DrawableOption<Drawable> option = null, bool pushToDrawStack = true)
            {
                var drawable = Context.CreateDrawable(type, Parent, pushToDrawStack ? DrawStack : null, option);
                Drawables.Add(drawable as T);

                return drawable;
            }

            public D Create<D>(DrawableOption<D> option = null, bool pushToDrawStack = true) where D : T, new()
            {
                var drawable = Context.CreateDrawable(Parent, pushToDrawStack ? DrawStack : null, option);
                Drawables.Add(drawable);

                return drawable;
            }

            public int Destroy<D>() where D : T
            {
                var destroyCount = 0;
                foreach(var drawable in Drawables)
                {
                    if(drawable is D)
                    {
                        Context.DestroyDrawable(drawable, DrawStack);
                        destroyCount++;
                    }
                }

                Drawables.RemoveAll(drawable => drawable is D);

                return destroyCount;
            }

            public void Destroy(T drawable)
            {
                Context.DestroyDrawable(drawable, DrawStack);
                Drawables.Remove(drawable);
            }

            public IEnumerable<D> Get<D>() where D : T
            {
                return Drawables.FindAll(drawable => drawable is D).Cast<D>();
            }
        }
    }
}
