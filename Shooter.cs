using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CollisionDetectionExample
{
    internal class Shooter
    {

        float ShootInterval = 0.2f;
        float cooldown;

        public Bullet Update(float deltaTime, Vector2 origin)
        {
            Vector2 direction = Vector2.Zero;
            cooldown = MathF.Max(0f, cooldown - deltaTime);

            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Left)) direction.X -= 1;
            if (keyboard.IsKeyDown(Keys.Right)) direction.X += 1;
            if (keyboard.IsKeyDown(Keys.Up)) direction.Y -= 1;
            if (keyboard.IsKeyDown(Keys.Down)) direction.Y += 1;

            //Gamepad Shooting
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            if(direction == Vector2.Zero && gamePad.IsConnected)
            {
                Vector2 stick = gamePad.ThumbSticks.Right;

                if (stick.LengthSquared() > 0.3f * 0.3f)
                {
                    float angle = MathF.Atan2(-stick.Y, stick.X);

                    float snappedAngle = MathF.Round(angle/ MathHelper.PiOver4) * MathHelper.PiOver4;

                    direction = new Vector2(MathF.Cos(snappedAngle), MathF.Sin(snappedAngle));
                }
            }

            if(direction == Vector2.Zero || cooldown > 0f)
            {
                return null;
            }

            cooldown = ShootInterval;
            return new Bullet(origin, direction);
        }
    }
}
