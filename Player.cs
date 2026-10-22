using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CollisionDetectionExample
{
    internal class Player
    {
        const int size = 64;
        const float speed = 220f;

        Vector2 position;

        public SpriteAnimation IdleAnimation { get; set; }
        public SpriteAnimation RunAnimation { get; set; }
        SpriteAnimation currentAnimation;
        SpriteEffects facing = SpriteEffects.None;

        public Rectangle Bounds => new Rectangle((int)MathF.Round(position.X), (int)MathF.Round(position.Y), size,size);

        public bool IsColliding { get; private set; }

        public Player(Vector2 startPosition)
        {
            this.position = startPosition;
        }

        public void Update(float deltaTime, Wall[] walls, Rectangle playArea)
        {
            //Button Inputs
            KeyboardState keyboard = Keyboard.GetState();
           
            Vector2 direction = Vector2.Zero;
            if (keyboard.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S)) direction.Y += 1;
            if (keyboard.IsKeyDown(Keys.A)) direction.X -= 1;
            if (keyboard.IsKeyDown(Keys.D)) direction.X += 1;

            //check to seeif controller is connnected
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

            if (gamePad.IsConnected)
            {
                direction.X += gamePad.ThumbSticks.Left.X;
                direction.Y -= gamePad.ThumbSticks.Left.Y;
                if(gamePad.DPad.Up == ButtonState.Pressed) direction.Y -= 1;
                if(gamePad.DPad.Down == ButtonState.Pressed) direction.Y += 1;
                if(gamePad.DPad.Left == ButtonState.Pressed) direction.X -= 1;
                if(gamePad.DPad.Right == ButtonState.Pressed) direction.X += 1;
            }

            //Normalize direction
            if (direction != Vector2.Zero)
                direction.Normalize();

            //Flip sprite based on direction
            if (direction.X < 0)
                facing = SpriteEffects.FlipHorizontally;
            if (direction.X > 0)
                facing = SpriteEffects.None;

            SpriteAnimation nextAnimation = direction != Vector2.Zero ? RunAnimation : IdleAnimation;

            if (nextAnimation != currentAnimation)
            {
                currentAnimation = nextAnimation;
                currentAnimation?.Reset();
            }

            currentAnimation?.Update(deltaTime);

            IsColliding = false;
            //Move player Clamp to play area
            Vector2 movement = direction * speed * deltaTime;

            //Movement plus Collsion Detection
            int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(movement.X), MathF.Abs(movement.Y))/8f));
            Vector2 step = movement / steps;


            for(int i = 0; i < steps; i++)
            {
                position.X += step.X;

                foreach (Wall wall in walls)
                {
                    if(step.X != 0 && Bounds.Intersects(wall.Bounds))
                    {
                        IsColliding = true;
                        position.X = step.X >0 ? wall.Bounds.Left - size : wall.Bounds.Right;
                    }
                }

                position.Y += step.Y;

                foreach (Wall wall in walls)
                {
                    if (step.Y != 0 && Bounds.Intersects(wall.Bounds))
                    {
                        IsColliding = true;
                        position.Y = step.Y > 0 ? wall.Bounds.Top - size : wall.Bounds.Bottom;
                    }
                }
            }


            //position += movement;
            position.X = MathHelper.Clamp(position.X, playArea.Left, playArea.Right - size);
            position.Y = MathHelper.Clamp(position.Y, playArea.Top, playArea.Bottom - size);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
        {
            Color tint = IsColliding ? Color.Tomato : Color.White;

            if (currentAnimation != null)
            {
                currentAnimation.Draw(spriteBatch, Bounds, tint, facing);
            }
            else
            {
                spriteBatch.Draw(whitePixel, Bounds, IsColliding ? Color.Tomato : Color.LimeGreen);
            }
                
        }
    }
}
