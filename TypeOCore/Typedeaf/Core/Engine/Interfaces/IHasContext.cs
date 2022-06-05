namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        public interface IHasContext
        {
            public Context Context { get; set; }

            public void SetContext(Context context)
            {
                Context = context;
            }
        }
    }
}
