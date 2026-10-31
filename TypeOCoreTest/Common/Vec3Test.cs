using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOCoreTest.Common;

public class Vec3Test
{
    [Fact]
    public void ConstructorsAndMutatorsSetComponents()
    {
        Assert.Equal(new Vec3(2, 2, 2), new Vec3(2));
        Assert.Equal(new Vec3(1, 2, 3), new Vec3(1, 2, 3));
        Assert.Equal(new Vec3(4, 5, 6), new Vec3(new Vec2(4, 5), 6));
        Assert.Equal(new Vec3(4, 5, 0), new Vec3(new Vec2(4, 5)));

        var vector = new Vec3(1, 2, 3)
            .SetX(4)
            .SetY(new Vec3(0, 5, 0))
            .SetZ(6)
            .Set(7, 8, 9)
            .Transform(1, 1, 1)
            .TransformZ(new Vec3(0, 0, 1));
        Assert.Equal(new Vec3(8, 9, 11), vector);
    }

    [Fact]
    public void ArithmeticAndEqualityOperateOnComponents()
    {
        Assert.Equal(new Vec3(5, 7, 9), new Vec3(1, 2, 3) + new Vec3(4, 5, 6));
        Assert.Equal(new Vec3(-3, -3, -3), new Vec3(1, 2, 3) - new Vec3(4, 5, 6));
        Assert.Equal(new Vec3(2, 6, 12), new Vec3(1, 2, 3) * new Vec3(2, 3, 4));
        Assert.Equal(new Vec3(2, 3, 4), new Vec3(4, 9, 16) / new Vec3(2, 3, 4));
        Assert.Equal(new Vec3(2, 3, 4), new Vec3(4, 6, 8) / 2);
        Assert.Equal(new Vec3(6, 3, 2), 12 / new Vec3(2, 4, 6));
        Assert.True(new Vec3(1, 2, 3) == new Vec3(1, 2, 3));
        Assert.True(new Vec3(1, 2, 3) != new Vec3(3, 2, 1));
    }

    [Fact]
    public void GeometryHelpersReturnExpectedValues()
    {
        var vector = new Vec3(1, 2, 2);
        Assert.Equal(9, vector.LengthSquared());
        Assert.Equal(3, vector.Length());
        Assert.Equal(3, vector.Distance(new Vec3(2, 4, 4)));
        Assert.Equal(new Vec3(0, 0, 1), Vec3.Cross(new Vec3(1, 0, 0), new Vec3(0, 1, 0)));
        Assert.Equal(11, Vec3.Dot(new Vec3(1, 2, 3), new Vec3(1, 2, 2)));

        var normalized = new Vec3(0, 3, 4);
        normalized.Normalize();
        Assert.Equal(0, normalized.X, 10);
        Assert.Equal(0.6, normalized.Y, 10);
        Assert.Equal(0.8, normalized.Z, 10);
        var zero = Vec3.Zero;
        zero.Normalize();
        Assert.Equal(Vec3.Zero, zero);
    }
}
