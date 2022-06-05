namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        public class Matrix
        {
            private readonly System.Drawing.Drawing2D.Matrix _matrix;

            public Matrix()
            {
                _matrix = new System.Drawing.Drawing2D.Matrix();
            }

            public Vec2 Translation {
                get { return new Vec2(_matrix.OffsetX, _matrix.OffsetY); }
                set { Translate(value - Translation); }
            }

            public void Translate(double translateX, double translateY)
            {
                _matrix.Translate((float)translateX, (float)translateY);
            }

            public void Translate(Vec2 translate)
            {
                Translate(translate.X, translate.Y);
            }

            public Vec2 Transform(Vec2 transform)
            {
                var points = new System.Drawing.PointF[] { new System.Drawing.PointF((float)transform.X, (float)transform.Y) };
                _matrix.TransformPoints(points);
                return new Vec2(points[0].X, points[0].Y);
            }
        }
    }
}
