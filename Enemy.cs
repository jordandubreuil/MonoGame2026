using Microsoft.Xna.Framework;  
using Microsoft.Xna.Framework.Graphics;  
using System;
using System.Diagnostics;


namespace CollisionDetectionExample
{
    public enum EnemyState { Idle, Chase, Dead };
    public enum EnemyMovementType { BackAndForth, Chase, Waypoints, Wander}
    internal class Enemy
    {
        public const int Size = 64
            ;
        private Vector2 position;
        readonly SpriteAnimation idleAnimation;
        readonly SpriteAnimation chaseAnimation;
        readonly SpriteAnimation deathAnimation;
        SpriteEffects facing = SpriteEffects.None;
        float hitFlashTime;
        const float ChaseRecheckInterval = 0.5f;
        float chaseRecheckTimer;
        int patrolDirection = 1;
        float directionChangeTimer;

        public EnemyState State { get; private set; } = EnemyState.Idle;
        public int HitsRemaining { get; private set; }
        public int MaxHits { get; }
        public bool hasFinishedDying => State == EnemyState.Dead && deathAnimation.IsFinished;
        public float DetectionRange { get; set; } = 200f;
        public EnemyMovementType MovementType { get; }
        public Color Tint { get; set; }
        
        //These are the bounds for horizontal patrolling
        public float LeftBound { get; set; }
        public float RightBound { get; set; }
        public float ChangeDirectionInterval { get; set; } = 1.5f;
        public Rectangle MovementArea { get; set; } = new Rectangle(0, 0, 800, 600);
        public float Speed { get; set; } = 85f;
        public Rectangle Bounds => new Rectangle((int)MathF.Round(position.X), (int)MathF.Round(position.Y), Size, Size);

        public Enemy(Vector2 startPosition, SpriteAnimation idleAnimation, SpriteAnimation chaseAnimation, SpriteAnimation deathAnimation, int hitsToKill = 3, EnemyMovementType movementType = EnemyMovementType.Chase)
        {
            if (hitsToKill < 1) throw new ArgumentOutOfRangeException(nameof(hitsToKill));
            position = startPosition;
            this.idleAnimation = idleAnimation;
            this.chaseAnimation = chaseAnimation;
            this.deathAnimation = deathAnimation;
            MaxHits = hitsToKill;
            HitsRemaining = hitsToKill;
            MovementType = movementType;
            LeftBound = startPosition.X - 60;
            RightBound = startPosition.X + 60;
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

            if(MovementType != EnemyMovementType.Chase)
            {
                
                UpdatePatrol(deltaTime, playerBounds, walls);
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

        void UpdatePatrol(float deltaTime, Rectangle playerBounds, Wall[] walls)
        {
            Vector2 movement = Vector2.Zero;
            switch (MovementType)
            {
                case EnemyMovementType.BackAndForth:
                    if (LeftBound >= RightBound) break;
                    if (position.X >= RightBound) patrolDirection = -1;
                    if (position.X <= LeftBound) patrolDirection = 1;

                    float targetX = patrolDirection > 0 ? RightBound : LeftBound;
                    float distanceX = targetX - position.X;
                    movement.X = MathF.Sign(distanceX) * MathF.Min(Speed * deltaTime, MathF.Abs(distanceX));
                    Debug.WriteLine(movement.X);
                    break;
            }

            Vector2 oldPosition = position;
            bool hitWall = MovePatrol(movement, playerBounds, walls);
            if (hitWall)
            {
                if(MovementType == EnemyMovementType.BackAndForth)
                {
                    patrolDirection = movement.X > 0 ? -1 : 1;
                }
            }

            Vector2 actualMovement = position - oldPosition;
            if (actualMovement.X < 0) facing = SpriteEffects.FlipHorizontally;
            if (actualMovement.X > 0) facing = SpriteEffects.None;

            EnemyState nextState = actualMovement == Vector2.Zero ? EnemyState.Idle : EnemyState.Chase;
            if(nextState != State)
            {
                State = nextState;
                if(State == EnemyState.Idle)idleAnimation.Reset();
                else chaseAnimation.Reset();
            }

            if(State == EnemyState.Idle)
            {
                idleAnimation.Update(deltaTime);
            }
            else
            {
                chaseAnimation.Update(deltaTime);
            }
            
        }

        bool MovePatrol(Vector2 movement, Rectangle playerBounds, Wall[] walls)
        {
            int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(movement.X), MathF.Abs(movement.Y)) / 4f));
            Vector2 step = movement / steps;
            for(int i = 0; i < steps; i++)
            {
                Vector2 previous = position;
                if(TouchesWalls(walls) || MovementArea.Contains(Bounds))
                {
                    position = previous;
                    return true;
                }

                if (Bounds.Intersects(playerBounds))
                {
                    position.X = previous.X;
                }

                float oldY = position.Y;
                position.Y = step.Y;

                if(TouchesWalls(walls) || !MovementArea.Contains(Bounds))
                {
                    position.Y = oldY;
                    return true;
                }

                if (Bounds.Intersects(playerBounds))
                {
                    position.Y = oldY;
                }
            }
            return false;
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
                    deathAnimation.Draw(spriteBatch, Bounds, Tint, facing); 
                }
                return;
            }

            SpriteAnimation animation = State == EnemyState.Idle ? idleAnimation : chaseAnimation;
            animation.Draw(spriteBatch, Bounds, hitFlashTime > 0 ? Color.Red : Tint, facing);


            //Health bar
            Rectangle bar = new Rectangle(Bounds.X, Bounds.Y - 6, Bounds.Width, 3);
            spriteBatch.Draw(whitePixel, bar, Color.DarkRed);
            bar.Width = Size * HitsRemaining / MaxHits;
            spriteBatch.Draw(whitePixel, bar, Color.LimeGreen);

        }


    }
}
