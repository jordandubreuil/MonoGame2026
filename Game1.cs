using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Diagnostics;

namespace CollisionDetectionExample
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D whitePixel;
        Texture2D spaceSprite;
        Song backgroudMusic;
        SoundEffect gunshotSound;
        SoundEffect explosionSound;
        Player player;
        Wall[] walls;
        Enemy enemy;
        EnemySpawner spawner;
        readonly List<Enemy> enemies = new List<Enemy>();
        private readonly Shooter shooter = new Shooter();
        private readonly List<Bullet> bullets = new List<Bullet>();

        float bgScrollX = 0f;
        float bgScrollSpeed = 100f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "WASD to Move Collsion Example - ESC to Quit";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            player = new Player(new Vector2(80, 80));

            //Setup the walls
            walls = new Wall[]
            {
                new Wall(new Rectangle(200,100,40,300)),
                new Wall(new Rectangle(360,220,260,40)),
                new Wall(new Rectangle(400,300,40,110)),
                new Wall(new Rectangle(480,80,200,40))
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
           whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            spaceSprite = Content.Load<Texture2D>("space");
           whitePixel.SetData(new Color[] { Color.White });
            Texture2D idleSheet = Content.Load<Texture2D>("Player Idle 48x48");
            Texture2D runSheet = Content.Load<Texture2D>("player run 48x48");
            Texture2D deathSheet = Content.Load<Texture2D>("Player Death 64x64");
            player.IdleAnimation = new SpriteAnimation(idleSheet, 48, 48, 0, 10, 8f, true);
            player.RunAnimation = new SpriteAnimation(runSheet, 48, 48, 0, 8, 8f, true);

            //Single Enemy
            //enemy = new Enemy(new Vector2(600, 300),
            //    new SpriteAnimation(idleSheet, 48, 48, 0, 10, 8f),
            //    new SpriteAnimation(runSheet, 48, 48, 0, 8, 8f),
            //    new SpriteAnimation(deathSheet, 48, 48, 0, 10, 10f, loop: false),
            //    hitsToKill: 3)
            //{ DetectionRange = 200f, Speed = 85f};

            //enemies.Add(enemy);

            Enemy CreateEnemy(Vector2 position, EnemyMovementType movementType, Color tint, Vector2[]waypoints = null)
            {
                return new Enemy(position,
                new SpriteAnimation(idleSheet, 48, 48, 0, 10, 8f),
                new SpriteAnimation(runSheet, 48, 48, 0, 8, 8f),
                new SpriteAnimation(deathSheet, 48, 48, 0, 10, 10f, loop: false),
                hitsToKill: 3, movementType: movementType, waypoints:waypoints)
                {
                    Tint = tint,
                    Speed = 85f,
                    DetectionRange = 200f,
                    MovementArea = GraphicsDevice.Viewport.Bounds
                };

            }

            //Create the back and forth enemy
            Enemy patrol = CreateEnemy(new Vector2(300,10), EnemyMovementType.BackAndForth, Color.CornflowerBlue);
            patrol.LeftBound = 280;
            patrol.RightBound = 650;
            enemies.Add(patrol);

            //Create Chase Enemy
            Enemy chase = CreateEnemy(new Vector2(300, 260), EnemyMovementType.Chase, Color.Orange);
            enemies.Add(chase);

            //Create Waypoint Enemy
            Enemy waypoint = CreateEnemy(new Vector2(500, 300), EnemyMovementType.Waypoints, Color.Violet, new Vector2[]
            {
                new Vector2(500,300),
                new Vector2(700,300),
                new Vector2(700,400),
                new Vector2(500,400),
            });

            enemies.Add(waypoint);

            //Create Wandering Enemy
            Enemy wanderer = CreateEnemy(new Vector2(75, 350), EnemyMovementType.Wander, Color.Yellow);
            wanderer.ChangeDirectionInterval = 1.5f;
            enemies.Add(wanderer);

            //spawner = new EnemySpawner(idleSheet, runSheet, deathSheet)
            //{
            //    HitsToKill = 3,
            //    DetectionRange = 200f,
            //    EnemySpeed = 85f
            //};
            //spawner.Update(enemies, GraphicsDevice.Viewport.Bounds, player.Bounds, walls);

            backgroudMusic = Content.Load<Song>("FutureAmbient_2");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.25f;
            MediaPlayer.Play(backgroudMusic);
            gunshotSound = Content.Load<SoundEffect>("bullet");
            explosionSound = Content.Load<SoundEffect>("explosion");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime, walls, GraphicsDevice.Viewport.Bounds);
            //enemy.Update(deltaTime, player.Bounds, walls);
            foreach(Enemy enemy in enemies)
            {
                
                enemy.Update(deltaTime, player.Bounds, walls);
            }

            bgScrollX += bgScrollSpeed * deltaTime;

            if (bgScrollX >= spaceSprite.Width)
            {
                bgScrollX -= spaceSprite.Width;
            }

            //Spawn a bullet
            Bullet shot = shooter.Update(deltaTime, player.Bounds.Center.ToVector2());
            if(shot != null)
            {
                bullets.Add(shot);
                gunshotSound.Play(volume:0.5f, pitch:0, pan:0);
            }

            //Iterate through bullets backward
            for(int i =  bullets.Count - 1; i >=0; i--)
            {
                bool hitEnemy = bullets[i].Update(deltaTime, walls, GraphicsDevice.Viewport.Bounds, enemies);
                if (hitEnemy)
                {
                    explosionSound.Play(volume: 0.5f, pitch: 0, pan: 0);
                }
                if (!bullets[i].IsActive)
                {
                    bullets.RemoveAt(i);
                }
            }

            enemies.RemoveAll(enemy => enemy.hasFinishedDying);
            //int previousWave = spawner.WaveNumber;
            //spawner.Update(enemies, GraphicsDevice.Viewport.Bounds, player.Bounds, walls);
            //if(spawner.WaveNumber != previousWave)
            //{
            //    bullets.Clear();
            //}

            //Window.Title = $"Wave: {spawner.WaveNumber} : Wave Size: {spawner.WaveSize}";

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(25,30,40));

            _spriteBatch.Begin();
            //Scrolling Background Example
            float bgX = -bgScrollX;
            _spriteBatch.Draw(spaceSprite, new Vector2(bgX, 0), Color.White);

            if (bgX + spaceSprite.Width < _graphics.PreferredBackBufferWidth)
            {
                _spriteBatch.Draw(spaceSprite, new Vector2(bgX + spaceSprite.Width, 0), Color.White);
            }

            player.Draw(_spriteBatch, whitePixel);
            //enemy.Draw(_spriteBatch, whitePixel);
            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(_spriteBatch, whitePixel);
            }
            //draw the walls
            foreach (Wall wall in walls)
            {
                wall.Draw(_spriteBatch, whitePixel);
            }

            foreach(Bullet bullet in bullets)
            {
                bullet.Draw(_spriteBatch, whitePixel);
            }

            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
