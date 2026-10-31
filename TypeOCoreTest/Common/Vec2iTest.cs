using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOCoreTest.Common;

public class Vec2iTest
{
    [Fact]
    public void ConstructorsAndMutatorsSetComponents()
    {
        Assert.Equal(new Vec2i(0, 0), default);
        Assert.Equal(new Vec2i(3, 3), new Vec2i(3));
        Assert.Equal(new Vec2i(2, 4), new Vec2i(2, 4));
        Assert.Equal(new Vec2i(2, -3), new Vec2i(new Vec2(2.9, -3.9)));

        var vector = new Vec2i(1, 2)
            .SetX(3)
            .SetY(new Vec2i(0, 4))
            .Set(5, 6)
            .TransformX(1)
            .TransformY(new Vec2i(0, 2))
            .Transform(1, 1);
        Assert.Equal(new Vec2i(7, 9), vector);
        Assert.Equal(new Vec2(7, 9), (Vec2)vector);
    }

    [Fact]
    public void ArithmeticAndEqualityOperateOnComponents()
    {
        Assert.Equal(new Vec2i(4, 6), new Vec2i(1, 2) + new Vec2i(3, 4));
        Assert.Equal(new Vec2i(3, 4), new Vec2i(1, 2) + 2);
        Assert.Equal(new Vec2i(2, 2), new Vec2i(5, 6) - new Vec2i(3, 4));
        Assert.Equal(new Vec2i(3, 8), new Vec2i(1, 2) * new Vec2i(3, 4));
        Assert.Equal(new Vec2i(2, 3), new Vec2i(5, 7) / new Vec2i(2, 2));
        Assert.Equal(new Vec2i(3, 4), new Vec2i(7, 9) / 2);
        Assert.Equal(new Vec2i(2, 4), new Vec2i(1, 2) * 2);

        Assert.True(new Vec2i(1, 2) == new Vec2i(1, 2));
        Assert.True(new Vec2i(1, 2) != new Vec2i(2, 1));
    }

    [Fact]
    public void GeometryHelpersAndConstantsReturnExpectedValues()
    {
        var vector = new Vec2i(3, 4);
        Assert.Equal(25, vector.LengthSquared());
        Assert.Equal(5, vector.Length());
        Assert.Equal(new Vec2i(3, 4), vector.Direction(new Vec2i(6, 8)));
        Assert.Equal(5, vector.Distance(new Vec2i(6, 8)));
        Assert.Equal(11, Vec2i.Dot(new Vec2i(1, 4), new Vec2i(3, 2)));
        Assert.Equal(new Vec2i(3, 4), Vec2i.Max(new Vec2i(1, 4), new Vec2i(3, 2)));
        Assert.Equal(new Vec2i(1, 2), Vec2i.Min(new Vec2i(1, 4), new Vec2i(3, 2)));
        Assert.Equal(new Vec2i(1, 1), Vec2i.One);
        Assert.Equal(new Vec2i(0, 0), Vec2i.Zero);
        Assert.Equal(new Vec2i(1, 0), Vec2i.UnitX);
        Assert.Equal(new Vec2i(0, 1), Vec2i.UnitY);
    }
}
