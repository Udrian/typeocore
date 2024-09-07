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

        public override Color PixelAt(int x, int y) { return Color.White; }

        public override void FlipVertical()
        {
            throw new NotImplementedException();
        }

        public override void FlipHorizontal()
        {
            throw new NotImplementedException();
        }
    }
}
