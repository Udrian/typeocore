namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>Represents a rectangle by its position and size.</summary>
        public struct Rectangle
        {
            /// <summary>Gets or sets the rectangle's position.</summary>
            public Vec2 Pos { get; set; }
            /// <summary>Gets or sets the rectangle's size.</summary>
            public Vec2 Size { get; set; }

            /// <summary>Initializes a rectangle from a position and size vector.</summary>
            public Rectangle(Vec2 pos, Vec2 size)
            {
                Pos = pos;
                Size = size;
            }

            /// <summary>Initializes a rectangle from its position and dimensions.</summary>
            public Rectangle(double x, double y, double width, double height)
            {
                Pos = new Vec2(x, y);
                Size = new Vec2(width, height);
            }
        }
    }
}