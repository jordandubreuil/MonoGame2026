using System;
using Microsoft.Xna.Framework;  
using Microsoft.Xna.Framework.Graphics;  


namespace CollisionDetectionExample
{
    public enum EnemyState { Idle, Chase, Dead };
    internal class Enemy
    {
        public const int Size = 64;
        private Vector2 position;
        readonly SpriteAnimation idleAnimation;
        readonly SpriteAnimation chaseAnimation;
        readonly SpriteAnimation deathAnimation;
        SpriteEffects facing = SpriteEffects.None;
        float hitFlashTime;
        const float ChaseRecheckInterval = 0.5f;
        float chaseRecheckTimer;

        public EnemyState State { get; private set; } = EnemyState.Idle;
        public int HitsRemaining { get; private set; }
        public int MaxHits { get; }
        public bool hasFinishedDying => State == EnemyState.Dead && deathAnimation.IsFinished;
        public float DetectionRange { get; set; } = 200f;
        public float Speed { get; set; } = 85f;
        public Rectangle Bounds => new Rectangle((int)MathF.Round(position.X), (int)MathF.Round(position.Y), Size, Size);

        public Enemy(Vector2 startPosition, SpriteAnimation idleAnimation, SpriteAnimation chaseAnimation, SpriteAnimation deathAnimation, int hitsToKill = 3)
        {
            if (hitsToKill < 1) throw new ArgumentOutOfRangeException(nameof(hitsToKill));
            position = startPosition;
            this.idleAnimation = idleAnimation;
            this.chaseAnimation = chaseAnimation;
            this.deathAnimation = deathAnimation;
            MaxHits = hitsToKill;
            HitsRemaining = hitsToKill;
        }

        public void Update(float deltaTime, Rectangle playerBounds, Wall[] walls)
        {
            hitFlashTime = MathF.Max(0, hitFlashTime - deltaTime);
            chaseRecheckTimer = MathF.Max(0, chaseRecheckTimer - deltaTime);

            if (State == EnemyState.Dead)
            {
                deathAnimation.Update(deltaTime);
                return;
            }

            //Check if the player is within detection range
            Vector2 toPlayer = playerBounds.Center.ToVector2() - Bounds.Center.ToVector2();
            bool playerInRange = toPlayer.LengthSquared() <= DetectionRange * DetectionRange;

            //Recheck player is in range
            if(State == EnemyState.Idle && playerInRange && chaseRecheckTimer <= 0)
            {
                chaseRecheckTimer = ChaseRecheckInterval;
                if(CanStepTowardPlayer(toPlayer, walls))
                {
                    State = EnemyState.Chase;
                    chaseAnimation.Reset();
                }
            }

            if(State == EnemyState.Idle)
            {
                idleAnimation.Update(deltaTime);
                return;
            }

            if(toPlayer.X< 0) facing = SpriteEffects.FlipHorizontally;
            else facing = SpriteEffects.None;

            Vector2 movement = toPlayer == Vector2.Zero? Vector2.Zero : 
                Vector2.Normalize(toPlayer) * MathF.Min(Speed * deltaTime, toPlayer.Length());
            int steps = Math.Min(1, (int)MathF.Ceiling(movement.Length() / 4f));

            Vector2 step = movement / steps;

            for(int i = 0; i < steps; i++)
            {
                float oldX = position.X;
                position.X += step.X;
                if (TouchesWalls(walls))
                {
                    position.X = oldX;
                    StopChasing();
                    return;
                }
                if(Bounds.Intersects(playerBounds))position.X = oldX;

                float oldY = position.Y;
                position.Y += step.Y;
                if (TouchesWalls(walls))
                {
                    position.Y = oldY;
                    StopChasing();
                    return;
                }
                if (Bounds.Intersects(playerBounds)) position.Y = oldY;
            }
            chaseAnimation.Update(deltaTime);
        }


        //Stop Chasing the Player
        void StopChasing()
        {
            State = EnemyState.Idle;
            chaseRecheckTimer = ChaseRecheckInterval;
            idleAnimation.Reset();
        }

        //Detect if the enemy is touching walls
        bool TouchesWalls(Wall[] walls)
        {
            return TouchesWalls(Bounds, walls);
        }

        static bool TouchesWalls(Rectangle bounds, Wall[] walls)
        {
            foreach (var wall in walls)
            {
                if (bounds.Intersects(wall.Bounds))
                    return true;
            }

            return false;
        }

        bool CanStepTowardPlayer(Vector2 toPlayer, Wall[] walls)
        {
            if(toPlayer == Vector2.Zero)return false;

            //Preview ahead of the enemy's position to see if it would collide with a wall
            Vector2 step = Vector2.Normalize(toPlayer) * MathF.Min(4f, toPlayer.Length());
            Rectangle candidate = new Rectangle(
                (int)MathF.Round(position.X + step.X), 
                (int)MathF.Round(position.Y + step.Y), 
                Size, 
                Size);

            if (TouchesWalls(candidate, walls))
                return false;
            candidate.Y = (int)MathF.Round(position.Y + step.Y);
            return !TouchesWalls(candidate, walls);
        }

        public bool TakeHit()
        {
            if (State == EnemyState.Dead) return false;
            HitsRemaining--;
            hitFlashTime = 0.12f;
            if(HitsRemaining == 0)
            {
                State = EnemyState.Dead;
                deathAnimation.Reset();
            }
            return true;
        }   

        public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
        {
            if(State == EnemyState.Dead)
            {
                if (!deathAnimation.IsFinished)
                {
                    deathAnimation.Draw(spriteBatch, Bounds, Color.Orange, facing); 
                }
                return;
            }

            SpriteAnimation animation = State == EnemyState.Idle ? idleAnimation : chaseAnimation;
            animation.Draw(spriteBatch, Bounds, hitFlashTime > 0 ? Color.Red : Color.Orange, facing);


            //Health bar
            Rectangle bar = new Rectangle(Bounds.X, Bounds.Y - 6, Bounds.Width, 3);
            spriteBatch.Draw(whitePixel, bar, Color.DarkRed);
            bar.Width = Size * HitsRemaining / MaxHits;
            spriteBatch.Draw(whitePixel, bar, Color.LimeGreen);

        }


    }
}
