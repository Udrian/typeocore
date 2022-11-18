using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        /// <summary>
        /// Abstract base class for Font content
        /// </summary>
        public abstract class Font : Content
        {
            /// <summary>
            /// Size of Font
            /// </summary>
            public virtual int FontSize { get; set; }


            /// <summary>
            /// Do not call directly, should be loaded through ContentLoader.LoadContent
            /// </summary>
            protected Font() { }

            /// <summary>
            /// Measures the string based on the font, font size and text applied
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public abstract Vec2 MeasureString(string text);
        }
    }
}