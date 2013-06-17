using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;


namespace Dungeon
{
    public static class Config 
    {
        //Number of rows and cols the game will consist off
        public static int ROWS = 11;
        public static int COLUMNS = 19;
        public static bool DEBUG = false;
        public static int LOGLEVEL = 1;
        public static float SCALE = 0.6f;
        public static Vector2 offset = new Vector2(0.0f, 0.0f);
        public static App_Log applog = new App_Log("log.log");
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //Width and height of each sprite        

        public static int SPRITE_WIDTH = (int)(64 * Config.SCALE);
        public static int SPRITE_HEIGHT = (int)(64 * Config.SCALE);
        public static float TIME_TO_MOVE = 2.0f;
        private int x = 0;
        private int y = 0;
        private Level level;
       
        private SpriteFont hudFont;
        GraphicsDeviceManager graphics;
        Texture2D dummyText;
        Rectangle dummyRect;
        SpriteBatch spriteBatch;
        Rectangle titleSafeArea;
        Vector2 hudLocation;
        List<Tile> redraw;

        float timeToMove = TIME_TO_MOVE;
        float timeToSpawn = 30.0f;

        string levelPath;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //size of the screen, number of sprites times sprite size
            graphics.PreferredBackBufferWidth = Config.COLUMNS*SPRITE_WIDTH;
            graphics.PreferredBackBufferHeight = Config.ROWS * SPRITE_HEIGHT;
            
            Content.RootDirectory = "Content";
            levelPath = String.Format("Levels/1.txt");
            levelPath = "Content/" + levelPath;
            
        }

            
        
        /// <summary> 
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.IsMouseVisible = true;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            dummyText = Content.Load<Texture2D>("stone");
            
            dummyRect = new Rectangle();
            dummyRect.Width = SPRITE_WIDTH;
            dummyRect.Height = SPRITE_HEIGHT;

            
            redraw = new List<Tile>();

            hudFont = Content.Load<SpriteFont>("Hud");
            level = new Level(Services, levelPath);
            // TODO: use this.Content to load your game content here
            titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);

            foreach(Tile tile in (level.getTiles()))
            {
                redraw.Add(tile);
            }
            GraphicsDevice.Clear(Color.CornflowerBlue);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            level.Dispose();
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || (Keyboard.GetState()).IsKeyDown(Keys.Escape))
                this.Exit();

            //Handle the timing to move the "AI" 
            if (timeToMove > 0)
            {
                timeToMove -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeToMove <= 0)
                {
                    
                    
                    level.move(gameTime);
                    timeToMove = TIME_TO_MOVE;
                }
            }
            //Handle the timing to spawn a new AI if possible
            if (timeToSpawn > 0)
            {
                timeToSpawn -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeToSpawn <= 0)
                {
                    level.spawn(gameTime);
                    timeToSpawn = 3000.0f; //TODO: change back to 30.0f
                }
            }
            //Generate a new level 
            if ((Keyboard.GetState()).IsKeyDown(Keys.N))
            {
                // Generate a new level, switch to that one. 
                if (level.generateLevel())
                {
                    levelPath = String.Format("Levels/tmp.txt");
                    levelPath = "Content/" + levelPath;
                }
                LoadContent();
            }
            // "Camera" movement
            if ((Keyboard.GetState()).IsKeyDown(Keys.Left))
            {
                Config.offset.X -= 1.0f;
            }
            if ((Keyboard.GetState()).IsKeyDown(Keys.Right))
            {
                Config.offset.X += 1.0f;
            }
            if ((Keyboard.GetState()).IsKeyDown(Keys.Down))
            {
                Config.offset.Y -= 1.0f;
            }
            if ((Keyboard.GetState()).IsKeyDown(Keys.Up))
            {
                Config.offset.Y += 1.0f;
            }
            // TODO: Add your update logic here
            
            base.Update(gameTime);
        }

        private void HandleInput()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                
                x = (int)(mouseState.X-Config.offset.X)/SPRITE_WIDTH;
                y = (int)(mouseState.Y-Config.offset.Y)/SPRITE_HEIGHT;
                if (x > level.Width || y > level.Height || x < -1 || y < -1)
                {
                    return;
                }
                dummyRect.X = (x + (int)Config.offset.X ) * SPRITE_WIDTH;
                dummyRect.Y = (y + (int)Config.offset.Y) * SPRITE_HEIGHT;
                if (level.checkTile(x, y) == TileCollision.MINEABLE)
                {
                    level.changeTexture(x, y, Content.Load<Texture2D>("stone"));
                    spriteBatch.Begin();
                    spriteBatch.DrawString(hudFont, x.ToString(), hudLocation + Config.offset, Color.Black);
                    spriteBatch.Draw(dummyText, dummyRect , new Color(255,255,255,120)); //Works at origo but not with offset?
                    spriteBatch.End();
                }
                //something was clicked, mark it? 

            }
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            spriteBatch.Begin();
            //send a queue with the tiles that needs to be redrawn
            level.Draw(spriteBatch, redraw);
            spriteBatch.End();
            HandleInput();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
