using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Contents
    {
        /// <summary>
        /// Abstract base class for Texture content
        /// </summary>
        public abstract class Texture : Content
        {
            /// <summary>
            /// Pixel size of Texture
            /// </summary>
            public Vec2i Size { get; protected set; }

            /// <summary>
            /// Do not call directly, should be loaded through ContentLoader.LoadContent
            /// </summary>
            protected Texture() { }

            /// <summary>
            /// Create the texture with the given size and byte array data, should be called through ContentLoader.CreateTexture
            /// </summary>
            /// <param name="size">Size of the texture</param>
            /// <param name="data">Data containing the actual image</param>
            protected abstract void Create(Vec2i size, ReadOnlySpan<byte> data);
            internal void InternalCreate(Vec2i size, ReadOnlySpan<byte> data)
            {
                Size = size;
                Create(size, data);
            }

            /// <summary>
            /// Saves the texture to disk
            /// </summary>
            /// <param name="path">File path to the saved texture</param>
            public abstract void Save(string path);
        }
    }
}