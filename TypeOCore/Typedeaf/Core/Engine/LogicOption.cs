namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>Provides options for creating a logic component.</summary>
        /// <typeparam name="L">The logic type to configure.</typeparam>
        public class LogicOption<L> : CreateOption<L> where L : Logic
        {
            /// <summary>Applies this option to a logic instance.</summary>
            public override bool Create(L obj)
            {
                return true;
            }
        }
    }
}
