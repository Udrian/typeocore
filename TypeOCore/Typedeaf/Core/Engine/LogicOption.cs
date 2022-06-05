namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public class LogicOption<L> : CreateOption<L> where L : Logic
        {
            public override bool Create(L obj)
            {
                return true;
            }
        }
    }
}
