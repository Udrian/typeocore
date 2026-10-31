using TypeOEngine.Typedeaf.Core.Common;

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

        [Fact]
        public void TestAddition()
        {
            Assert.Equal(new Vec2(4, 6), new Vec2(1, 2) + new Vec2(3, 4));
            Assert.Equal(new Vec2(3, 4), new Vec2(1, 2) + 2);
            Assert.Equal(new Vec2(3, 4), 2 + new Vec2(1, 2));
        }

        [Fact]
        public void TestSubtraction()
        {
            Assert.Equal(new Vec2(2, 2), new Vec2(5, 6) - new Vec2(3, 4));
            Assert.Equal(new Vec2(3, 4), new Vec2(5, 6) - 2);
            Assert.Equal(new Vec2(3, 4), 5 - new Vec2(2, 1));
            Assert.Equal(new Vec2(-1, -2), -new Vec2(1, 2));
        }

        [Fact]
        public void TestMultiplication()
        {
            Assert.Equal(new Vec2(3, 8), new Vec2(1, 2) * new Vec2(3, 4));
            Assert.Equal(new Vec2(2, 4), new Vec2(1, 2) * 2);
            Assert.Equal(new Vec2(2, 4), 2 * new Vec2(1, 2));
        }

        [Fact]
        public void TestDivision()
        {
            Assert.Equal(new Vec2(2, 2), new Vec2(6, 8) / new Vec2(3, 4));
            Assert.Equal(new Vec2(3, 4), new Vec2(6, 8) / 2);
            Assert.Equal(new Vec2(3, 2), 6 / new Vec2(2, 3));
        }

        [Fact]
        public void TestEquality()
        {
            Assert.True(new Vec2(1, 2) == new Vec2(1, 2));
            Assert.False(new Vec2(1, 2) != new Vec2(1, 2));
            Assert.False(new Vec2(1, 2) == new Vec2(2, 1));
            Assert.True(new Vec2(1, 2) != new Vec2(2, 1));
        }

        [Fact]
        public void TestLengthAndDistance()
        {
            var vector = new Vec2(3, 4);
            Assert.Equal(25, vector.LengthSquared());
            Assert.Equal(5, vector.Length());
            Assert.Equal(5, vector.Distance(new Vec2(6, 8)));
            Assert.Equal(25, vector.DistanceSquared(new Vec2(6, 8)));
            Assert.Equal(5, Vec2.Distance(new Vec2(3, 4), new Vec2(6, 8)));
            Assert.Equal(25, Vec2.DistanceSquared(new Vec2(3, 4), new Vec2(6, 8)));
        }

        [Fact]
        public void TestDirectionNormalizeAndRotate()
        {
            Assert.Equal(new Vec2(3, 4), new Vec2(1, 2).Direction(new Vec2(4, 6)));
            Assert.Equal(new Vec2(3, 4), Vec2.Direction(new Vec2(1, 2), new Vec2(4, 6)));
            var normalized = new Vec2(3, 4).Normalize();
            Assert.Equal(0.6, normalized.X, 10);
            Assert.Equal(0.8, normalized.Y, 10);
            Assert.Equal(Vec2.Zero, Vec2.Zero.Normalize());
            var rotated = new Vec2(1, 0).Rotate(Math.PI / 2);
            Assert.Equal(0, rotated.X, 10);
            Assert.Equal(1, rotated.Y, 10);
        }

        [Fact]
        public void TestDot()
        {
            var left = new Vec2(1, 4);
            var right = new Vec2(3, 2);
            Assert.Equal(11, left.Dot(right));
            Assert.Equal(11, Vec2.Dot(left, right));
            Assert.Equal(new Vec2(3, 4), Vec2.Max(left, right));
            Assert.Equal(new Vec2(1, 2), Vec2.Min(left, right));
            Assert.Equal(new Vec2(1, 1), Vec2.One);
            Assert.Equal(new Vec2(0, 0), Vec2.Zero);
            Assert.Equal(new Vec2(1, 0), Vec2.UnitX);
            Assert.Equal(new Vec2(0, 1), Vec2.UnitY);
        }

        [Fact]
        public void TestSetTransformAndConversions()
        {
            var vector = new Vec2(1, 2)
                .SetX(3)
                .SetY(new Vec2(0, 4))
                .Set(5, 6)
                .TransformX(1)
                .TransformY(new Vec2(0, 2))
                .Transform(1, 1);
            Assert.Equal(new Vec2(7, 9), vector);

            Vec2i integer = (Vec2i)new Vec2(2.9, -3.9);
            Assert.Equal(new Vec2i(2, -3), integer);
            Assert.Equal(new Vec2(2, -3), (Vec2)integer);
        }
    }
}
