namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        public interface IHasContext
        {
            public Context Context { get; internal set; }

            internal void SetContext(Context context)
            {
                Context = context;
            }
        }
    }
}
