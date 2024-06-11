using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents.ContentExtensions
    {
        /// <summary>
        /// Extender class for extending Texture content loading
        /// </summary>
        public static class ContentTextureExtension
        {
            /// <summary>
            /// Create a texture given the provided data
            /// </summary>
            /// <typeparam name="T">Type of Texture content to create</typeparam>
            /// <param name="contentLoader">ContentLoader</param>
            /// <param name="size">Size of the texture</param>
            /// <param name="data">Data containing the actual image</param>
            /// <returns>The newly created Texture</returns>
            public static T CreateTexture<T>(this ContentLoader contentLoader, Vec2i size, ReadOnlySpan<byte> data) where T : Texture
            {
                var texture = contentLoader.CreateContent<T>(null);
                if (texture == null) return null;

                texture.InternalCreate(size, data);
                return texture;
            }
        }
    }
}
