using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOCoreTest.Common;

public class GeometryTests
{
    [Fact]
    public void MatrixTranslatesAndTransformsPoints()
    {
        var matrix = new Matrix();
        matrix.Translate(2, 3);
        matrix.Translate(new Vec2(1, -1));

        Assert.Equal(new Vec2(3, 2), matrix.Translation);
        Assert.Equal(new Vec2(5, 6), matrix.Transform(2, 4));
        Assert.Equal(new Vec2(5, 6), matrix.Transform(new Vec2(2, 4)));

        matrix.Translation = new Vec2(0, 0);
        Assert.Equal(Vec2.Zero, matrix.Translation);
    }

    [Fact]
    public void RectangleConstructorsPreservePositionAndSize()
    {
        var byVectors = new Rectangle(new Vec2(1, 2), new Vec2(3, 4));
        var byDimensions = new Rectangle(1, 2, 3, 4);

        Assert.Equal(byVectors.Pos, byDimensions.Pos);
        Assert.Equal(byVectors.Size, byDimensions.Size);
    }

    [Fact]
    public void RayReportsForwardPlaneIntersectionAndIgnoresParallelOrBehindPlanes()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(0, 0, 1));
        var forwardPlane = new Plane(new Vec3(0, 0, 1), -5);
        var behindPlane = new Plane(new Vec3(0, 0, 1), 2);
        var parallelPlane = new Plane(new Vec3(1, 0, 0), -1);

        Assert.Equal(5, ray.Intersects(forwardPlane));
        Assert.Null(ray.Intersects(behindPlane));
        Assert.Null(ray.Intersects(parallelPlane));
        Assert.Equal(new Vec3(0, 0, 1), forwardPlane.Normal);
    }

    [Fact]
    public void ColorSupportsNormalizedChannelsEqualityAndClampedInterpolation()
    {
        var color = new Color(0.5, 0.25, 0.75);
        Assert.Equal((byte)255, color.A);
        Assert.Equal((byte)127, color.R);
        Assert.Equal((byte)63, color.G);
        Assert.Equal((byte)191, color.B);
        Assert.Equal(color, new Color(color.A, color.R, color.G, color.B));
        Assert.Equal(color, Color.Lerp(color, new Color(0, 0, 0, 0), 0));
        Assert.Equal(new Color(0, 0, 0, 0), Color.Lerp(color, new Color(0, 0, 0, 0), 2));
        Assert.Equal((byte)127, Color.Lerp(new Color(0, 0, 0, 0), new Color(255, 255, 255, 255), 0.5).R);
    }
}
