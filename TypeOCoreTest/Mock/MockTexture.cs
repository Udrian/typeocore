using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOCoreTest.Mock
{
    internal class MockTexture : Texture
    {
        protected override void Create(Vec2i size, ReadOnlySpan<byte> data) { }

        protected override void Load(string path) { }

        protected override void Cleanup() { }

        public override void Save(string path) {}
    }
}
