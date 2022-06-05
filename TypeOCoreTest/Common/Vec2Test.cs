using TypeOEngine.Typedeaf.Core.Common;
using Xunit;

namespace TypeOCoreTest.Common
{
    public class Vec2Test
    {
        [Fact]
        public void TestCreation()
        {
            var v = new Vec2();
            Assert.Equal(0, v.X);
            Assert.Equal(0, v.Y);

            v = new Vec2(1);
            Assert.Equal(1, v.X);
            Assert.Equal(1, v.Y);

            v = new Vec2(2, 3);
            Assert.Equal(2, v.X);
            Assert.Equal(3, v.Y);

            v = new Vec2(new Vec2(4, 5));
            Assert.Equal(4, v.X);
            Assert.Equal(5, v.Y);

            v = new Vec2();
            v = v.Set(6, 7);
            Assert.Equal(6, v.X);
            Assert.Equal(7, v.Y);

            v = new Vec2();
            v = v.Set(new Vec2(8, 9));
            Assert.Equal(8, v.X);
            Assert.Equal(9, v.Y);

            v = new Vec2
            {
                X = 10,
                Y = 11
            };
            Assert.Equal(10, v.X);
            Assert.Equal(11, v.Y);
        }

        /*[Fact]
        public void TestAddition()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestSubtraction()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestMultiplication()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestDivision()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestEqulity()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestLenght()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public void TestDistance()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestNormalize()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestRotation()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestDot()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestMax()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void TestMin()
        {
            throw new NotImplementedException();
        }*/
    }
}
