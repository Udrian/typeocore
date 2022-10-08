namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        /// <summary>
        /// Abstract bath class for all contents, sound, texture, fonts etc.
        /// </summary>
        public abstract class Content : TypeObject
        {
            /// <summary>
            /// Path to the file that have been used to load the content
            /// </summary>
            public string FilePath { get; internal set; }

            /// <inheritdoc/>
            protected override void Initialize() { }

            /// <summary>
            /// Load the content with the provided path to file
            /// </summary>
            /// <param name="path">Path to the content file to load</param>
            /// <param name="contentLoader">ContentLoader used to load the file</param>
            public abstract void Load(string path, ContentLoader contentLoader);
        }
    }
}
