using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        /// <summary>
        /// Class for handling and loading content, such as images, music and fonts
        /// </summary>
        public class ContentLoader : TypeOObject, IHasContext
        {
            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            private ILogger Logger { get; set; }

            /// <summary>
            /// Set base loading path
            /// </summary>
            public string BasePath { get; set; }
            private Dictionary<Type, Type> ContentBinding { get; set; }

            internal ContentLoader(Dictionary<Type, Type> contentBinding)
            {
                BasePath = Directory.GetCurrentDirectory();
                ContentBinding = contentBinding;
            }

            protected override void Initialize()
            {
            }

            protected override void Cleanup()
            {
            }

            /// <summary>
            /// Loads a content of specified type
            /// </summary>
            /// <typeparam name="C">The content to load</typeparam>
            /// <param name="path">The path to the content, BasePath is appended to this path</param>
            /// <returns>The loaded content</returns>
            public C LoadContent<C>(string path) where C : Content
            {
                path = Path.Combine(BasePath, path);

                Content content = CreateContent<C>(path);
                if (content == null) return null;

                content.FilePath = path;
                content.InternalLoad(path);
                return (C)content;
            }

            internal C CreateContent<C>(string path) where C : Content
            {
                if(path != null && !File.Exists(path))
                {
                    Logger.Log(LogLevel.Fatal, $"File of type '{typeof(C).FullName}' does not exists with path '{path}'");
                    return null;
                }

                Content content;
                if (ContentBinding.ContainsKey(typeof(C)))
                {
                    Logger.Log(LogLevel.Debug, $"Loading content path '{path}' of type '{typeof(C).FullName}' bound to type '{ContentBinding[typeof(C)].FullName}'");
                    content = Activator.CreateInstance(ContentBinding[typeof(C)]) as Content;
                }
                else
                {
                    if (typeof(C).IsAbstract)
                    {
                        var message = $"Base content type '{typeof(C).Name}' is missing a sub class Content Binding";
                        Logger.Log(LogLevel.Fatal, message);
                        return null;
                    }
                    Logger.Log(LogLevel.Debug, $"Loading content path '{path}' of type '{typeof(C).FullName}'");
                    content = Activator.CreateInstance(typeof(C)) as Content;
                }

                Context.InitializeObject(content);

                return (C)content;
            }
        }
    }
}