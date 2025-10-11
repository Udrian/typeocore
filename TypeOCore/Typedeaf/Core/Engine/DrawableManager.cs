using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Manages a collection of drawable objects of a specified type, providing functionality to create, retrieve,
        /// and destroy them.
        /// </summary>
        /// <remarks>This class is designed to manage drawable objects within a specific context and
        /// parent object. It provides methods to create new drawables, retrieve existing ones, and destroy them as
        /// needed. The manager also integrates with a draw stack to control the rendering order of
        /// drawables.</remarks>
        /// <typeparam name="T">The type of drawable objects managed by this instance. Must derive from <see cref="Drawable"/>.</typeparam>
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

            /// <summary>
            /// Creates a new drawable of the specified type and optionally adds it to the draw stack.
            /// </summary>
            /// <param name="type">The type of the drawable to create. Must derive from <see cref="Drawable"/>.</param>
            /// <param name="option">Optional configuration for the drawable being created. Can be <see langword="null"/>.</param>
            /// <param name="pushToDrawStack">A value indicating whether the created drawable should be added to the draw stack.  <see
            /// langword="true"/> to add it to the draw stack; otherwise, <see langword="false"/>.</param>
            /// <returns>The created drawable instance.</returns>
            public Drawable Create(Type type, DrawableOption<Drawable> option = null, bool pushToDrawStack = true)
            {
                var drawable = Context.CreateDrawable(type, Parent, pushToDrawStack ? DrawStack : null, option);
                Drawables.Add(drawable as T);

                return drawable;
            }

            /// <summary>
            /// Creates a new drawable object of the specified type.
            /// </summary>
            /// <typeparam name="D">The type of the drawable object to create. Must derive from <typeparamref name="T"/> and have a
            /// parameterless constructor.</typeparam>
            /// <param name="option">An optional configuration object used to customize the creation of the drawable. If null, default
            /// settings are applied.</param>
            /// <param name="pushToDrawStack">A value indicating whether the created drawable should be added to the draw stack. Defaults to <see
            /// langword="true"/>.</param>
            /// <returns>The newly created drawable object of type <typeparamref name="D"/>.</returns>
            public D Create<D>(DrawableOption<D> option = null, bool pushToDrawStack = true) where D : T, new()
            {
                var drawable = Context.CreateDrawable(Parent, pushToDrawStack ? DrawStack : null, option);
                Drawables.Add(drawable);

                return drawable;
            }

            /// <summary>
            /// Destroys all drawable objects of the specified type and removes them from the collection.
            /// </summary>
            /// <remarks>This method iterates through the collection of drawable objects, identifies
            /// those of the specified type, and invokes the destruction process for each. After destruction, the
            /// objects are removed from the collection.</remarks>
            /// <typeparam name="D">The type of drawable objects to destroy. Must derive from <typeparamref name="T"/>.</typeparam>
            /// <returns>The number of drawable objects that were destroyed and removed from the collection.</returns>
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

            /// <summary>
            /// Destroys the specified drawable and removes it from the collection of managed drawables.
            /// </summary>
            /// <remarks>This method ensures that the specified drawable is properly destroyed using
            /// the associated context and then removes it from the internal collection. The caller is responsible for
            /// ensuring that the drawable is no longer needed before calling this method.</remarks>
            /// <param name="drawable">The drawable to be destroyed. Cannot be null.</param>
            public void Destroy(T drawable)
            {
                Context.DestroyDrawable(drawable, DrawStack);
                Drawables.Remove(drawable);
            }

            /// <summary>
            /// Retrieves all elements of the specified type from the collection.
            /// </summary>
            /// <remarks>This method filters the collection to include only elements that are of the
            /// specified type <typeparamref name="D"/> or a derived type. The returned collection is a casted
            /// enumeration of those elements.</remarks>
            /// <typeparam name="D">The type of elements to retrieve. Must be a type derived from <typeparamref name="T"/>.</typeparam>
            /// <returns>An <see cref="IEnumerable{T}"/> containing all elements of type <typeparamref name="D"/> in the
            /// collection. If no elements of the specified type are found, the returned collection is empty.</returns>
            public IEnumerable<D> Get<D>() where D : T
            {
                return Drawables.FindAll(drawable => drawable is D).Cast<D>();
            }

            /// <summary>
            /// Removes all drawable objects from the collection and releases their associated resources.
            /// </summary>
            /// <remarks>This method destroys each drawable object in the collection using the
            /// associated context  before clearing the collection. After calling this method, the collection will be
            /// empty, and all resources tied to the drawable objects will have been released.</remarks>
            public void Clear()
            {
                foreach (var drawable in Drawables)
                {
                    Context.DestroyDrawable(drawable, DrawStack);
                }
                Drawables.Clear();
            }
        }
    }
}
