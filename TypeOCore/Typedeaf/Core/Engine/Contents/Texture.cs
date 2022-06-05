using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        public abstract class Texture : Content
        {
            public Vec2 Size { get; protected set; }

            protected Texture() { }
        }
    }
}