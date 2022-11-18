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
            public Vec2 Size { get; protected set; }

            /// <summary>
            /// Do not call directly, should be loaded through ContentLoader.LoadContent
            /// </summary>
            protected Texture() { }
        }
    }
}