using System;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Interfaces
    {
        public interface IDrawable : IComparable<IDrawable>
        {
            public bool Hidden { get; set; }
            public int DrawOrder { get; set; }
            public void Draw(Canvas canvas);

            int IComparable<IDrawable>.CompareTo(IDrawable other)
            {
                if(other == null) return 1;

                return DrawOrder.CompareTo(other.DrawOrder);
            }
        }
    }
}
