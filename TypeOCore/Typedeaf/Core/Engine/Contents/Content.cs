namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        /// <summary>
        /// Abstract bath class for all contents, sound, texture, fonts etc.
        /// </summary>
        public abstract class Content : TypeOObject
        {
            /// <summary>
            /// Path to the file that have been used to load the content
            /// </summary>
            public string FilePath { get; internal set; }

            /// <summary>
            /// Do not call directly, should be loaded through ContentLoader.LoadContent
            /// </summary>
            protected Content() { }

            /// <inheritdoc/>
            protected override void Initialize() { }
            
            /// <summary>
            /// Load the content with the provided path to file
            /// </summary>
            /// <param name="path">Path to the content file to load</param>
            protected abstract void Load(string path);
            internal void InternalLoad(string path) { Load(path); }
        }
    }
}
