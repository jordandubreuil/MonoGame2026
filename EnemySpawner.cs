using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BigInteger = System.Numerics.BigInteger;

namespace CollisionDetectionExample
{
    internal class EnemySpawner
    {
        Random random = new Random();
        Texture2D idleSheet;
        Texture2D walkSheet;
        Texture2D deathSheet;

        //BIGINTEGER keeps the doubling overflow going without ending th spawning
        public BigInteger WaveSize { get; private set; }
        public BigInteger PendingSpawns { get; private set; }
        public int WaveNumber { get;  set; }
        public int HitsToKill { get;  set; } = 3;
        public float DetectionRange { get;  set; } = 200f;
        public float EnemySpeed { get;  set; } = 85f;

        public EnemySpawner(Texture2D idleSheet, Texture2D walkSheet, Texture2D deathSheet)
        {
            this.idleSheet = idleSheet;
            this.walkSheet = walkSheet;
            this.deathSheet = deathSheet;
        }

        public void Update(List<Enemy> enemies, Rectangle playArea, Rectangle playerBounds, Wall[] walls)
        {
            if (enemies.Count == 0 && PendingSpawns == 0)
            {
                WaveNumber++;
                WaveSize = WaveSize == 0 ? BigInteger.One : WaveSize * 2;
                PendingSpawns = WaveSize;
            }
            if (PendingSpawns == 0) return;

            List<Vector2> positions = new List<Vector2>();

            for(int y = playArea.Top; y <= playArea.Bottom - Enemy.Size; y+= Enemy.Size)
            {
                for (int x = playArea.Left; x <= playArea.Right - Enemy.Size; x += Enemy.Size)
                {
                    positions.Add(new Vector2(x, y));
                }

            }

            while (PendingSpawns > 0 && positions.Count > 0)
            {
                int index = random.Next(positions.Count);
                Vector2 position = positions[index];
                positions.RemoveAt(index);
                Rectangle bounds = new Rectangle((int)position.X, (int)position.Y, Enemy.Size, Enemy.Size);
                if (!CanSpawn(bounds, playerBounds, walls, enemies)) continue;

                enemies.Add(new Enemy(
                    position,
                    new SpriteAnimation(idleSheet, 48, 48, 0, 10, 8f),
                    new SpriteAnimation(walkSheet, 48, 48, 0, 8, 8f),
                    new SpriteAnimation(deathSheet, 48, 48, 0, 10, 10f, loop: false),
                    HitsToKill
                    )
                {
                    DetectionRange = DetectionRange,
                    Speed = EnemySpeed
                });
                PendingSpawns--;
            }
        }

        private static bool CanSpawn(Rectangle bounds, Rectangle playerBounds, Wall[] walls, List<Enemy> enemies )
        {
            if(bounds.Intersects(playerBounds)) return false;
            foreach(Wall wall in walls)
            {
                if (bounds.Intersects(wall.Bounds)) return false;
            }
            foreach (Enemy enemy in enemies)
            {
                if (bounds.Intersects(enemy.Bounds)) return false;
            }
            return true;
        }
    }
}
