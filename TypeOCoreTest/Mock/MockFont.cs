using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOCoreTest.Mock
{
    internal class MockFont : Font
    {
        protected override void Load(string path) { }

        public override Vec2 MeasureString(string text) { return Vec2.Zero; }

        protected override void Cleanup() { }
    }
}
