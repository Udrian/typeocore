namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>Represents a two-dimensional translation matrix.</summary>
        public class Matrix
        {
            private readonly MathNet.Numerics.LinearAlgebra.Double.DenseMatrix _matrix;

            /// <summary>Initializes an identity transformation.</summary>
            public Matrix()
            {
                // Create a identity matrix
                _matrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.CreateIdentity(3);
            }

            /// <summary>Gets or sets the translation component of the matrix.</summary>
            public Vec2 Translation {
                get { return new Vec2(_matrix[0, 2], _matrix[1, 2]); }
                set { Translate(value - Translation); }
            }

            /// <summary>Offsets the matrix translation by the specified coordinates.</summary>
            public void Translate(double translateX, double translateY)
            {
                // TODO: Need to fix so that we actually use the matrix
                _matrix[0, 2] += translateX;
                _matrix[1, 2] += translateY;
            }

            /// <summary>Offsets the matrix translation by the specified vector.</summary>
            public void Translate(Vec2 translate)
            {
                Translate(translate.X, translate.Y);
            }
            /// <summary>Transforms the specified coordinates by this matrix.</summary>
            public Vec2 Transform(double transformX, double transformY)
            {
                var translateVec = MathNet.Numerics.LinearAlgebra.Double.DenseVector.OfArray(new double[] { transformX, transformY, 1 });
                var result = _matrix.Multiply(translateVec);
                return new Vec2(result[0], result[1]);
            }

            /// <summary>Transforms the specified vector by this matrix.</summary>
            public Vec2 Transform(Vec2 transform)
            {
                return Transform(transform.X, transform.Y);
            }
        }
    }
}
