using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Interfaces
    {
        public interface IDrawable : IComparable<IDrawable>
        {

            /// <summary>
            /// Gets or sets a value indicating whether the component is hidden.
            /// </summary>
            [TypeOProperty("Gets or sets a value indicating whether the component is hidden.", false)]
            public bool Hidden { get; set; }

            /// <summary>
            /// Gets or sets the draw order of the object.
            /// </summary>
            [TypeOProperty("Gets or sets the draw order of the object.", 0)]
            public int DrawOrder { get; set; }
            
            public void Draw(ICanvas canvas);

            int IComparable<IDrawable>.CompareTo(IDrawable other)
            {
                if(other == null) return 1;

                return DrawOrder.CompareTo(other.DrawOrder);
            }
        }
    }
}
