using System.Collections.Generic;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Graphics
    {
        public abstract class Canvas
        {
            public IWindow Window { get; set; }
            public abstract Rectangle Viewport { get; set; }
            public Matrix WorldMatrix { get; private set; }

            protected Canvas()
            {
                WorldMatrix = new Matrix();
            }

            public abstract void Initialize();
            public abstract void Cleanup();

            public abstract void Clear(Color clearColor);
            public abstract void DrawLine(Vec2 from, Vec2 size, Color color, IAnchor2d anchor = null);
            public abstract void DrawLineE(Vec2 from, Vec2 to, Color color, IAnchor2d anchor = null);
            public abstract void DrawLines(List<Vec2> points, Color color, IAnchor2d anchor = null);
            public abstract void DrawPixel(Vec2 point, Color color, IAnchor2d anchor = null);
            public abstract void DrawPixels(List<Vec2> points, Color color, IAnchor2d anchor = null);
            public abstract void DrawRectangle(Rectangle rectangle, bool filled, Color color, IAnchor2d anchor = null);
            public abstract void DrawRectangle(Vec2 from, Vec2 size, bool filled, Color color, IAnchor2d anchor = null);
            public abstract void DrawRectangleE(Vec2 from, Vec2 to, bool filled, Color color, IAnchor2d anchor = null);

            public abstract void DrawImage(Texture texture, Vec2 pos, IAnchor2d anchor = null);
            public abstract void DrawImage(Texture texture, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null);

            public abstract void DrawText(Font font, string text, Vec2 pos, IAnchor2d anchor = null);
            public abstract void DrawText(Font font, string text, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null);

            public abstract void Present();
        }
    }
}
