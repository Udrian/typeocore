using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Manages the lifecycle of <see cref="Logic"/> instances, including their creation, retrieval, and destruction.
        /// </summary>
        /// <remarks>The <see cref="LogicManager"/> class provides methods to create, retrieve, and
        /// destroy instances of <see cref="Logic"/> within a specific context. It maintains an internal collection of
        /// <see cref="Logic"/> objects and integrates with an <see cref="UpdateLoop"/> to manage their updates if
        /// required. This class is typically used in scenarios where multiple logic components need to be managed in a
        /// structured and consistent manner.</remarks>
        public class LogicManager : TypeOObject, IHasContext
        {
            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            internal List<Logic> Logics { get; private set; }
            private UpdateLoop UpdateLoop { get; set; }
            private TypeOObject Parent { get; set; }

            internal LogicManager(UpdateLoop updateLoop, TypeOObject parent)
            {
                Logics = new List<Logic>();
                UpdateLoop = updateLoop;
                Parent = parent;
            }

            protected override void Initialize()
            {
            }

            protected override void Cleanup()
            {
            }

            /// <summary>
            /// Creates and initializes a new instance of the specified logic type.
            /// </summary>
            /// <typeparam name="L">The type of logic to create. Must inherit from <see cref="Logic"/> and have a parameterless constructor.</typeparam>
            /// <param name="option">An optional <see cref="LogicOption{L}"/> instance that provides configuration for the created logic. If
            /// null, default options are used.</param>
            /// <param name="pushToUpdateLoop">A value indicating whether the created logic should be added to the update loop. The default value is
            /// <see langword="true"/>.</param>
            /// <returns>A new instance of the specified logic type, initialized with the provided options and added to the
            /// update loop if specified.</returns>
            public L Create<L>(LogicOption<L> option = null, bool pushToUpdateLoop = true) where L : Logic, new()
            {
                var logic = Context.CreateLogic<L>(Parent, pushToUpdateLoop ? UpdateLoop : null, option);
                Logics.Add(logic);
                return logic;
            }

            /// <summary>
            /// Destroys all instances of the specified logic type and removes them from the collection.
            /// </summary>
            /// <remarks>This method iterates through the collection of logics, destroys all instances
            /// of the specified type  using the associated context, and then removes them from the collection.
            /// The method returns the count of destroyed instances.</remarks>
            /// <typeparam name="L">The type of logic to destroy. Must derive from <see cref="Logic"/>.</typeparam>
            /// <returns>The number of logic instances that were destroyed and removed.</returns>
            public int Destroy<L>() where L : Logic
            {
                var destroyCount = 0;
                foreach(var logic in Logics)
                {
                    if(logic is L)
                    {
                        Context.DestroyLogic(logic, UpdateLoop);
                        destroyCount++;
                    }
                }

                Logics.RemoveAll(logic => logic is L);

                return destroyCount;
            }

            /// <summary>
            /// Destroys the specified logic instance, removing it from the current context and update loop.
            /// </summary>
            /// <remarks>This method removes the specified logic from the update loop and the internal
            /// collection of logics. Ensure that the logic instance is no longer needed before calling this method, as
            /// it will be permanently removed.</remarks>
            /// <param name="logic">The logic instance to be destroyed. Cannot be <see langword="null"/>.</param>
            public void Destroy(Logic logic)
            {
                Context.DestroyLogic(logic, UpdateLoop);
                Logics.Remove(logic);
            }

            /// <summary>
            /// Retrieves all instances of the specified logic type from the collection.
            /// </summary>
            /// <remarks>This method filters the collection to include only elements that are of the
            /// specified type <typeparamref name="L"/>. The returned collection is cast to the specified type.</remarks>
            /// <typeparam name="L">The type of logic to retrieve. Must derive from <see cref="Logic"/>.</typeparam>
            /// <returns>An <see cref="IEnumerable{T}"/> containing all instances of type <typeparamref name="L"/> found in the
            /// collection. If no instances are found, returns an empty collection.</returns>
            public IEnumerable<L> Get<L>() where L : Logic
            {
                return Logics.FindAll(logic => logic is L).Cast<L>();
            }
        }
    }
}
