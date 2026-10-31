namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        /// <summary>Provides access to a TypeO runtime context.</summary>
        public interface IHasContext
        {
            /// <summary>Gets or sets the associated runtime context.</summary>
            public Context Context { get; internal set; }

            internal void SetContext(Context context)
            {
                Context = context;
            }
        }
    }
}
