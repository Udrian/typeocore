using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        public abstract class Font : Content
        {
            public virtual int FontSize { get; set; }

            protected Font() { }

            public abstract Vec2 MeasureString(string text);
        }
    }
}