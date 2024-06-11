using static TypeOCoreTest.TypeOTest;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Contents.ContentExtensions;
using TypeOEngine.Typedeaf.Core.Common;
using System.IO;
using TypeOCoreTest.Mock;

namespace TypeOCoreTest.Contents
{
    public class ContentLoadTest
    {
        [Fact]
        public void LoadTexture()
        {
            var typeO = TypeO.Create<TestGame>("Test")
                .BindContent<Texture, MockTexture>() as TypeO;
            typeO.Start();

            Assert.NotNull(typeO.Context.ContentBinding[typeof(Texture)]);

            var textureNotExisting = typeO.Context.Game.ContentLoader.LoadContent<Texture>("nonexisting/path/to/texture");
            Assert.Null(textureNotExisting);

            var texture = typeO.Context.Game.ContentLoader.LoadContent<Texture>("Mock/Content/texture.png");
            Assert.NotNull(texture);
            Assert.IsType<MockTexture>(texture);
            Assert.Equal(Path.Combine(typeO.Context.Game.ContentLoader.BasePath, "Mock/Content/texture.png"), texture.FilePath);
        }

        [Fact]
        public void CreateTexture()
        {
            var typeO = TypeO.Create<TestGame>("Test")
                .BindContent<Texture, MockTexture>() as TypeO;
            typeO.Start();

            Assert.NotNull(typeO.Context.ContentBinding[typeof(Texture)]);

            var texture = typeO.Context.Game.ContentLoader.CreateTexture<Texture>(new Vec2i(100, 100), new ReadOnlySpan<byte>());
            Assert.NotNull(texture);
            Assert.IsType<MockTexture>(texture);
            Assert.Null(texture.FilePath);
            Assert.Equal(new Vec2i(100, 100), texture.Size);
        }

        [Fact]
        public void LoadFont()
        {
            var typeO = TypeO.Create<TestGame>("Test")
                .BindContent<Font, MockFont>() as TypeO;
            typeO.Start();

            Assert.NotNull(typeO.Context.ContentBinding[typeof(Font)]);

            var font = typeO.Context.Game.ContentLoader.LoadContent<Font>("Mock/Content/Lato-Black.ttf");
            Assert.NotNull(font);
            Assert.IsType<MockFont>(font);
            Assert.Equal(Path.Combine(typeO.Context.Game.ContentLoader.BasePath, "Mock/Content/Lato-Black.ttf"), font.FilePath);
            Assert.Equal(0, font.FontSize);

            var fontWithSize = typeO.Context.Game.ContentLoader.LoadContent<Font>("Mock/Content/Lato-Black.ttf", 25);
            Assert.NotNull(fontWithSize);
            Assert.IsType<MockFont>(fontWithSize);
            Assert.Equal(Path.Combine(typeO.Context.Game.ContentLoader.BasePath, "Mock/Content/Lato-Black.ttf"), fontWithSize.FilePath);
            Assert.Equal(25, fontWithSize.FontSize);
        }
    }
}
