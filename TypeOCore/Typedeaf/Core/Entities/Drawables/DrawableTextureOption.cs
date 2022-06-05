﻿using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Drawables
    {
        public class DrawableTextureOption : Drawable2dOption<DrawableTexture>
        {
            public Texture Texture { get; set; }

            public override bool Create(DrawableTexture obj)
            {
                if(!base.Create(obj)) return false;

                if(Texture != null)
                    obj.Texture = Texture;

                return true;
            }
        }
    }
}
