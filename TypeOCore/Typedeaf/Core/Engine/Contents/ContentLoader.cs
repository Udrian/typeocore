using System;
using System.Collections.Generic;
using System.IO;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        /// <summary>
        /// Class for handling and loading content, such as images, music and fonts
        /// </summary>
        public class ContentLoader : IHasContext
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

            /// <summary>
            /// Loads a content of specified type
            /// </summary>
            /// <typeparam name="C">The content to load</typeparam>
            /// <param name="path">The path to the content, BasePath is appended to this path</param>
            /// <returns>The loaded content</returns>
            /// <exception cref="Exception"></exception>
            public C LoadContent<C>(string path) where C : Content
            {
                path = Path.Combine(BasePath, path);

                Content content;
                if(ContentBinding.ContainsKey(typeof(C)))
                {
                    Logger.Log(LogLevel.Debug, $"Loading content path '{path}' of type '{typeof(C).FullName}' bound to type '{ContentBinding[typeof(C)].FullName}'");
                    content = Activator.CreateInstance(ContentBinding[typeof(C)]) as Content;
                }
                else
                {
                    if(typeof(C).IsAbstract)
                    {
                        var message = $"Base content type '{typeof(C).Name}' is missing a sub class Content Binding";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new Exception(message);
                    }
                    Logger.Log(LogLevel.Debug, $"Loading content path '{path}' of type '{typeof(C).FullName}'");
                    content = Activator.CreateInstance(typeof(C)) as Content;
                }

                Context.InitializeObject(content);

                content.FilePath = path;
                content.Load(path, this);
                return content as C;
            }
        }
    }
}