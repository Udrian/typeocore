using System.Collections.Generic;
using System.Linq;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public class LogicManager : IHasContext
        {
            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            internal List<Logic> Logics { get; private set; }
            private UpdateLoop UpdateLoop { get; set; }
            private object Parent { get; set; }

            internal LogicManager(UpdateLoop updateLoop, object parent)
            {
                Logics = new List<Logic>();
                UpdateLoop = updateLoop;
                Parent = parent;
            }

            public L Create<L>(LogicOption<L> option = null, bool pushToUpdateLoop = true) where L : Logic, new()
            {
                var logic = Context.CreateLogic<L>(Parent, pushToUpdateLoop ? UpdateLoop : null, option);
                Logics.Add(logic);
                return logic;
            }

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

            public void Destroy(Logic logic)
            {
                Context.DestroyLogic(logic, UpdateLoop);
                Logics.Remove(logic);
            }

            public IEnumerable<L> Get<L>() where L : Logic
            {
                return Logics.FindAll(logic => logic is L).Cast<L>();
            }
        }
    }
}
