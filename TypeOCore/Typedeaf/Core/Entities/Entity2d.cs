using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities
    {
        public abstract class Entity2d : Entity, IAnchor2d
        {
            public virtual Vec2   Position { get; set; }
            public virtual Vec2   Scale    { get; set; }
            public virtual double Rotation { get; set; }
            public virtual Vec2   Size     { get; set; }
            public virtual Vec2   Origin   { get; set; }

            public new Entity2d Parent { get { return base.Parent as Entity2d; } internal set { base.Parent = value as Entity2d; } }

            protected Entity2d() : base()
            {
                Position = Vec2.Zero;
                Scale    = Vec2.One;
                Rotation = 0;
                Size     = Vec2.Zero;
                Origin   = Vec2.Zero;
            }

            public Rectangle ScreenBounds {
                get {
                    return new Rectangle(
                           Position + (Parent?.ScreenBounds.Pos  ?? Vec2.Zero),
                           Size//     + (Parent?.DrawBounds.Size ?? Vec2.Zero)
                        );
                }
                set {
                    Position = value.Pos  - (Parent?.ScreenBounds.Pos  ?? Vec2.Zero);
                    Size     = value.Size;// - (Parent?.DrawBounds.Size ?? Vec2.Zero);
                }
            }

            public Anchor2d CreateAnchor(Vec2 anchorPosition, Orientation2d orientation = Orientation2d.UpperLeft, OrientationType orientationType = OrientationType.Absolute)
            {
                return new Anchor2d(anchorPosition, orientation, orientationType, this);
            }
        }
    }
}