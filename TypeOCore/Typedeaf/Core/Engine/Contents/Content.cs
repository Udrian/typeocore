namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        public abstract class Content
        {
            public string FilePath { get; protected set; }
            public abstract void Load(string path, ContentLoader contentLoader);
            public abstract void Cleanup();
        }
    }
}
