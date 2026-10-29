using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CollisionDetectionExample
{
    internal class Bullet
    {
        const int Size = 8;
        const float Speed = 450.0f;
        Vector2 position;
        Vector2 velocity;

        public bool IsActive { get; private set; } = true;

        public Rectangle Bounds => new Rectangle((int)MathF.Round(position.X), (int)MathF.Round(position.Y), Size, Size);

        public Bullet(Vector2 center, Vector2 direction)
        {
            if(direction == Vector2.Zero)
            {
                throw new ArgumentException("Bullets need a direction");
            }

            position = center - new Vector2(Size / 2f);
            velocity = Vector2.Normalize(direction) * Speed;
        }


        public bool Update(float deltaTime, Wall[] walls, Rectangle playeArea, List<Enemy> enemies) 
        {
            if (!IsActive) return false;

            Vector2 movement = velocity * deltaTime;

            int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(movement.X), MathF.Abs(movement.Y)) / 4f));

            Vector2 step = movement / steps;

            for (int i = 0; i <= steps; i++)
            {
                if (i > 0) position += step;
                //Collision with play area
                if (!Bounds.Intersects(playeArea))
                {
                    IsActive = false;
                    return false;
                }

                foreach(Wall wall in walls)
                {
                    if (Bounds.Intersects(wall.Bounds))
                    {
                        IsActive = false;
                        return false;
                    }
                }

                //Check for collision with enemies
                foreach (var enemy in enemies)
                {
                    if(enemy.State != EnemyState.Dead && Bounds.Intersects(enemy.Bounds))
                    {
                        IsActive = false;
                        return enemy.TakeHit();
                    }
                }
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
        {
            if (IsActive)
            {
                spriteBatch.Draw(whitePixel, Bounds, Color.Gold);
            }
        }



    }
}
