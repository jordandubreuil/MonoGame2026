using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CollisionDetectionExample
{
    internal class SpriteAnimation
    {
        Texture2D sheet;
        int frameWidth;
        int frameHeight;
        int frameCount;
        int firstFrame;
        double secondsPerFrame;
        bool loop;
        double elapsed;

        public int CurrentFrame { get; private set; }
        public bool IsFinished { get; private set; }

        public Rectangle SourceRectangle
        {
            get
            {
                int columns = sheet.Width / frameWidth;
                int index = firstFrame + CurrentFrame;
                return new Rectangle(index % columns * frameWidth, index / columns * frameHeight, frameWidth, frameHeight);
            }
        }

        public SpriteAnimation(Texture2D sheet, int frameWidth, int frameHeight, int firstFrame, int frameCount, float framesPerSecond, bool loop = true)
        {
            //Quality Error Handling
            if (sheet == null) throw new ArgumentNullException(nameof(sheet));
            if (frameWidth <= 0 || sheet.Width % frameWidth != 0) throw new ArgumentException("Frame width must be a positive integer that divides the sheet width evenly.", nameof(frameWidth));
            if (frameHeight <= 0 || sheet.Height % frameHeight != 0) throw new ArgumentException("Frame height must be a positive integer that divides the sheet height evenly.", nameof(frameHeight));
            int totalFrames = (sheet.Width / frameWidth) * (sheet.Height / frameHeight);
            if (firstFrame < 0 || firstFrame >= totalFrames) throw new ArgumentException(nameof(firstFrame));
            if (frameCount <= 0 || frameCount > totalFrames - firstFrame) throw new ArgumentException(nameof(frameCount));
            if (!float.IsFinite(framesPerSecond) || framesPerSecond <= 0) throw new ArgumentException(nameof(framesPerSecond));


            this.sheet = sheet;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.frameCount = frameCount;
            this.firstFrame = firstFrame;
            this.secondsPerFrame = 1.0 / framesPerSecond;
            this.loop = loop;
            this.elapsed = 0;
            this.CurrentFrame = 0;
            this.IsFinished = false;
        }

        public void Update(float deltaTime)
        {
            if (IsFinished) return;

            elapsed += deltaTime;

            while (elapsed >= secondsPerFrame)
            {
                elapsed -= secondsPerFrame;
                if (CurrentFrame < frameCount - 1)
                {
                    CurrentFrame++;
                }
                else if (loop)
                {
                    CurrentFrame = 0;
                }
                else
                {
                    IsFinished = true;
                    elapsed = 0;
                    break;
                }
            }
        }

        public void Reset()
        {
            CurrentFrame = 0;
            elapsed = 0;
            IsFinished = false;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle destination, Color tint, SpriteEffects effects = SpriteEffects.None)
        {
            spriteBatch.Draw(sheet, destination, SourceRectangle, tint, 0f, Vector2.Zero, effects,0f);
        }
    }
}
