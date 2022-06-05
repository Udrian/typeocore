using System;
using System.Collections.Generic;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        public class ContentLoader : IHasContext
        {
            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            protected ILogger Logger { get; set; }

            public string BasePath { get; set; }
            public Canvas Canvas { get; private set; }
            protected Dictionary<Type, Type> ContentBinding { get; private set; }

            protected ContentLoader(Canvas canvas, Dictionary<Type, Type> contentBinding)
            {
                Canvas = canvas;
                ContentBinding = contentBinding;
            }

            public C LoadContent<C>(string path) where C : Content
            {
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

                content.Load(path, this);
                return content as C;
            }
        }
    }
}