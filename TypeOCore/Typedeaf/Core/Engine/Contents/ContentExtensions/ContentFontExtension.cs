namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents.ContentExtensions
    {
        /// <summary>
        /// Extender class for extending Font content loading
        /// </summary>
        public static class ContentFontExtension
        {
            /// <summary>
            /// Loads a font of specified type with the given size
            /// </summary>
            /// <typeparam name="F">The font to load</typeparam>
            /// <param name="contentLoader">ContentLoader</param>
            /// <param name="path">The path to the content, BasePath is appended to this path</param>
            /// <param name="fontSize">Size of the font</param>
            /// <returns></returns>
            public static F LoadContent<F>(this ContentLoader contentLoader, string path, int fontSize) where F : Font
            {
                path = Path.Combine(contentLoader.BasePath, path);

                Font font = contentLoader.CreateContent<F>(path);
                if (font == null) return null;

                font.FilePath = path;
                font.FontSize = fontSize;
                font.InternalLoad(path);
                return (F)font;
            }
        }
    }
}
