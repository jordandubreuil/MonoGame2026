using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CollisionDetectionExample
{
    internal class Wall
    {
        public Rectangle Bounds { get; }

        public Wall(Rectangle bounds) 
        {
            Bounds = bounds;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
        {
            spriteBatch.Draw(whitePixel, Bounds, Color.SlateGray);
        }
    }
}
