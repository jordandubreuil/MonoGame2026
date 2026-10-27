using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace CollisionDetectionExample
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D whitePixel;
        Player player;
        Wall[] walls;
        private readonly Shooter shooter = new Shooter();
        private readonly List<Bullet> bullets = new List<Bullet>();


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
           whitePixel.SetData(new Color[] { Color.White });
            Texture2D idleSheet = Content.Load<Texture2D>("Player Idle 48x48");
            Texture2D runSheet = Content.Load<Texture2D>("player run 48x48");
            player.IdleAnimation = new SpriteAnimation(idleSheet, 48, 48, 0, 10, 8f, true);
            player.RunAnimation = new SpriteAnimation(runSheet, 48, 48, 0, 8, 8f, true);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime, walls, GraphicsDevice.Viewport.Bounds);

            //Spawn a bullet
            Bullet shot = shooter.Update(deltaTime, player.Bounds.Center.ToVector2());
            if(shot != null)
            {
                bullets.Add(shot);
            }

            //Iterate through bullets backward
            for(int i =  bullets.Count - 1; i >=0; i--)
            {
                bullets[i].Update(deltaTime, walls, GraphicsDevice.Viewport.Bounds);
                if (!bullets[i].IsActive)
                {
                    bullets.RemoveAt(i);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(25,30,40));

            _spriteBatch.Begin();
            player.Draw(_spriteBatch, whitePixel);
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
