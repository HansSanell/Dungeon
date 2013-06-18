using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Dungeon
{
    class Level : IDisposable
    {
        private Tile[,] tiles;
        private List<Vector2> spawnPoint = new List<Vector2>();
        private List<Vector2> startPoint = new List<Vector2>();

        private List<Tuple<Vector2, Enemy>> EnemyList = new List<Tuple<Vector2, Enemy>>();
        private List<Tuple<Vector2, Creature>> CreatureList = new List<Tuple<Vector2, Creature>>();
        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;
        
        public Level(IServiceProvider serviceProvider, string path)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");
            
            LoadTiles(path);
        }
        public Tile[,] getTiles()
        {
            return tiles;
        }
        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        private void LoadTiles(string path)
        {
            // Load the level and ensure all of the lines are the same length.
            int width = 0;
            List<string> lines = new List<string>();
            
            using (StreamReader reader = new StreamReader(path))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines {1}.", lines.Count, line));
                    line = reader.ReadLine();
                }
            }
            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
            
                    tiles[x, y] = LoadTile(tileType, x, y);
                    //Add the blue dot on these coordinates... TODO: remove the static initiation
                    if (x == 9 && y == 2)
                    {
                        //Add the blue dot just to have basic content
                        tiles[x, y].getTileContains().AddCreature(new Creature(Content.Load<Texture2D>("blue_dot"), "blue_dot"));
                        Config.applog.print(string.Format("//////////////////////////\n                // Starting new Dungeon //\n                //////////////////////////"));
                        
                    }

                }
            
            }
        }

        /// <summary>
        /// Translates the char in the level file to a tile object, texture name for each is named here. 
        /// </summary>
        /// <param name="tileType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            Vector2 pos = new Vector2(x, y);
            
            switch (tileType)
            {
                case '#':
                    return new Tile(TileType.STONE, Content.Load<Texture2D>("stone"), TileCollision.IMPASSABLE, pos, false);
                case 'G':
                    return new Tile(TileType.GOLD, Content.Load<Texture2D>("gold"), TileCollision.MINEABLE, pos, false);
                case 'L':
                    return new Tile(TileType.LAVA, Content.Load<Texture2D>("lava"), TileCollision.IMPASSABLE, pos, false);
                case 'R':
                    return new Tile(TileType.RUBBLE, Content.Load<Texture2D>("rubble"), TileCollision.MINEABLE, pos, false);
                case 'D':
                    return new Tile(TileType.DIRT, Content.Load<Texture2D>("tiled"), TileCollision.PASSABLE, pos, false);
                case 'O':
                    startPoint.Add(pos);
                    return new Tile(TileType.DIRT, content.Load<Texture2D>("tiled"), TileCollision.PASSABLE, pos, false);
                case 'S':
                    spawnPoint.Add(pos);
                    return new Tile(TileType.DIRT, Content.Load<Texture2D>("tiled"), TileCollision.PASSABLE, pos, false);
                default:
                    Config.applog.print(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }
        
        /// <summary>
        /// move all creatures
        /// </summary>
        public void move(GameTime gameTime)
        {
            Random rnd = new Random();
            
            Config.applog.print("into move", Config.LOGLEVEL);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (tiles[x, y].getTileContains().creatures.Count<Creature>() > 0)
                    {
                        List<Creature> tmp = new List<Creature>(tiles[x, y].getTileContains().creatures);
                        moveCreatures(rnd, x, y, tmp);
                       
                    }
                    if (tiles[x, y].getTileContains().enemies.Count<Enemy>() > 0)
                    {
                        List<Enemy> tmp = new List<Enemy>(tiles[x, y].getTileContains().enemies);
                        //tiles[x,y].getTileContains().enemies = moveEnemies(rnd, x, y, tmp);
                        moveEnemies(rnd, x, y, tmp);
                    }
                }
            }
            //Add loops for creature/enemy attack here. Not integrated with the move? 
        }
        //fix so that enemise and creature acts the same, enemise respawns after a fight, values don't match
        private void moveEnemies(Random rnd, int x, int y, List<Enemy> enemies)
        {
            //Add function call for moving creatures respecively enemies based on the list 
            Config.applog.print(string.Format("length: {0}", tiles[x, y].getTileContains().enemies.Count), Config.LOGLEVEL);

            foreach (Enemy enemy in enemies)
            {

                /*
                * get adjecent cells
                * If one contains an enemy, fight
                * check which one of these are traversable
                * random one of those
                * change the position of the "blob"
                * 
                * reading copy and working copy, merge after loop to avoid updating the loop (crashes)
                * 
                * Random a number depending on answer pick a direction (2 stays put)
                *          ^ 3
                *          |
                *          |    
                *   0 <--- 2 ---> 1
                *          |
                *          |
                *          v 4
                *  
                * send the enemy obj to battle function, check all adjecent tiles (diagonally?) 
                * deal damage accordin to ENEMY.battle
                * go to next tile/enemy
                * 
                * Enables "walk from battle" next turn for creature(otherwise dubbel damage each turn), or 
                * just trigger battle on enemy move, deal damage for both parties? 
                * 
                 * 
                 * Open issue: moving two enemies from the same tile to the removal queue will crash! 
                */
                
                if (enemy.moved == false)
                {

                    Config.applog.print(string.Format("name:{0}", enemy.name), Config.LOGLEVEL);
                    bool run = true;
                    Config.applog.print("got random", Config.LOGLEVEL);
                    int num = rnd.Next();
                    tiles[0, 0].setTexture(Content.Load<Texture2D>("dirt"));
                    Vector2 pos = new Vector2(x, y) ;
                    enemy.moved = true;
                    
                    switch (num % 5)
                    {
                        case 0:
                            Config.applog.print("case 0", Config.LOGLEVEL);
                            if (tiles[x - 1, y].getTileCollision().Equals(TileCollision.PASSABLE))
                            {
                                Config.applog.print("case 0 passable", Config.LOGLEVEL);

                                tiles[x - 1, y].getTileContains().AddEnemy(new Enemy(enemy));
                                tiles[x - 1, y].setTaken(TileCollision.TAKEN);
                                run = false;
                            }
                            break;
                        case 1:
                            Config.applog.print("case 1", Config.LOGLEVEL);
                            if (tiles[x + 1, y].getTileCollision().Equals(TileCollision.PASSABLE))
                            {
                                Config.applog.print("case 1 passable", Config.LOGLEVEL);
                                tiles[x + 1, y].getTileContains().AddEnemy(new Enemy(enemy));
                                tiles[x + 1, y].setTaken(TileCollision.TAKEN);
                                run = false;
                            }
                            break;
                        case 2:
                            Config.applog.print("case 2", Config.LOGLEVEL);
                            if (tiles[x, y].getTileCollision().Equals(TileCollision.PASSABLE))
                            {
                                Config.applog.print("case 2 passable", Config.LOGLEVEL);
                                tiles[x, y].setTaken(TileCollision.TAKEN);
                            }
                            break;
                        case 3:
                            Config.applog.print("case 3", Config.LOGLEVEL);
                            if (tiles[x, y - 1].getTileCollision().Equals(TileCollision.PASSABLE))
                            {
                                Config.applog.print("case 3 passable", Config.LOGLEVEL);
                                tiles[x, y - 1].getTileContains().AddEnemy(new Enemy(enemy));
                                tiles[x, y - 1].setTaken(TileCollision.TAKEN);
                                run = false;
                            }
                            break;
                        case 4:
                            Config.applog.print("case 4", Config.LOGLEVEL);
                            if (tiles[x, y + 1].getTileCollision().Equals(TileCollision.PASSABLE))
                            {
                                Config.applog.print("case 4 passable", Config.LOGLEVEL);
                                tiles[x, y + 1].getTileContains().AddEnemy(new Enemy(enemy));
                                tiles[x, y + 1].setTaken(TileCollision.TAKEN);
                                run = false;
                            }
                            break;
                    }
                    if (!run)
                    {
                        tiles[x, y].setPassable(TileCollision.PASSABLE);
                        tiles[x, y].getTileContains().RemoveEnemy(enemy);
                    }

                }
            }
        }
        private void moveCreatures(Random rnd, int x, int y, List<Creature> creatures)
        {
            //Add function call for moving creatures respecively enemies based on the list 
            Config.applog.print(string.Format("length: {0}", tiles[x, y].getTileContains().creatures.Count), Config.LOGLEVEL);

            foreach (Creature creature in creatures)
            {

                /*
                * get adjecent cells
                * If one contains an enemy, fight
                * check which one of these are traversable
                * random one of those
                * change the position of the "blob"
                * 
                * reading copy and working copy, merge after loop to avoid updating the loop (crashes)
                * 
                * Random a number depending on answer pick a direction (2 stays put)
                *          ^ 3
                *          |
                *          |    
                *   0 <--- 2 ---> 1
                *          |
                *          |
                *          v 4
                * 
                */
                if (creature.moved == true)
                {
                    return;
                }
                Config.applog.print(string.Format("name:{0}", creature.name), Config.LOGLEVEL);
                bool run = true;
                Config.applog.print("got random", Config.LOGLEVEL);
                int num = rnd.Next();
                creature.moved = true;
                
                Config.applog.print("entering battle", 1);

                switch (num % 5)
                {
                    case 0:
                        Config.applog.print("case 0", Config.LOGLEVEL);
                        if (tiles[x - 1, y].getTileCollision().Equals(TileCollision.PASSABLE))
                        {
                            Config.applog.print("case 0 passable", Config.LOGLEVEL);
                            //if the tile contains and enemy, battle both. check if that tile's enemy/creature has 0 or less in hp then remove. 
                            tiles[x - 1, y].getTileContains().AddCreature(new Creature(creature));
                            tiles[x - 1, y].setPassable(TileCollision.TAKEN);
                            run = false;
                        }
                        break;
                    case 1:
                        Config.applog.print("case 1", Config.LOGLEVEL);
                        if (tiles[x + 1, y].getTileCollision().Equals(TileCollision.PASSABLE))
                        {
                            Config.applog.print("case 1 passable", Config.LOGLEVEL);
                            tiles[x + 1, y].getTileContains().AddCreature(new Creature(creature));
                            tiles[x + 1, y].setPassable(TileCollision.TAKEN);
                            run = false;
                        }
                        break;
                    case 2:
                        Config.applog.print("case 2", Config.LOGLEVEL);
                        if (tiles[x, y].getTileCollision().Equals(TileCollision.PASSABLE))
                        {
                            Config.applog.print("case 2 passable", Config.LOGLEVEL);
                            tiles[x, y].setPassable(TileCollision.TAKEN);
                        }
                        break;
                    case 3:
                        Config.applog.print("case 3", Config.LOGLEVEL);
                        if (tiles[x, y - 1].getTileCollision().Equals(TileCollision.PASSABLE))
                        {
                            Config.applog.print("case 3 passable", Config.LOGLEVEL);
                            tiles[x, y - 1].getTileContains().AddCreature(new Creature(creature));
                            tiles[x, y - 1].setPassable(TileCollision.TAKEN);
                            run = false;
                        }
                        break;
                    case 4:
                        Config.applog.print("case 4", Config.LOGLEVEL);
                        if (tiles[x, y + 1].getTileCollision().Equals(TileCollision.PASSABLE))
                        {
                            Config.applog.print("case 4 passable", Config.LOGLEVEL);
                            tiles[x, y + 1].getTileContains().AddCreature(new Creature(creature));
                            tiles[x, y + 1].setPassable(TileCollision.TAKEN);
                            run = false;
                        }
                        break;
                }
                if (!run)
                {
                    tiles[x, y].setPassable(TileCollision.PASSABLE);
                    tiles[x, y].getTileContains().RemoveCreature(creature);
                }
            }
        }
        /// <summary>
        /// Draws all tiles at their corresponding position. 
        /// </summary>
        /// <param name="spriteBatch"></param>
        private void DrawTiles(SpriteBatch spriteBatch, List<Tile> redraw)
        {            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Texture2D texture = tiles[x, y].getTexture();
                    
                    if (texture != null)
                    {
                        Vector2 position = new Vector2(x, y) * tiles[x, y].size;

                        spriteBatch.Draw(texture, position + Config.offset, null, Color.White, 0.0f, Vector2.Zero, Config.SCALE, SpriteEffects.None, 1);
                        foreach (Creature creature in tiles[x,y].getTileContains().creatures) {
                            creature.moved = false;
                            //Add size of "blue_dot" here in case of changing tile-sizes... 
                            spriteBatch.Draw(creature.texture, position + Config.offset,null, Color.White,0.0f, Vector2.Zero, Config.SCALE - 0.25f, SpriteEffects.None, 1);
                        }
                        foreach (Enemy creature in tiles[x, y].getTileContains().enemies)
                        {
                            creature.moved = false;
                            //Add size of "blue_dot" here in case of changing tile-sizes... 
                            spriteBatch.Draw(creature.texture, position + Config.offset, null, Color.White, 0.0f, Vector2.Zero, Config.SCALE - 0.25f, SpriteEffects.None, 1);
                        }                    
                    }
                }
            }
        }
        /// <summary> 
        /// Change the texture on the tile at position x,y to given argument.
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="texture">the new texture</param>
        public void changeTexture(int x, int y, Texture2D texture)
        {
            tiles[x, y].setTexture(texture);
            
        }
        /// <summary>
        /// Check if tile is passable (wall or floor)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TileCollision checkTile(int x, int y)
        {
            return tiles[x, y].getTileCollision();
        }
        /// <summary>
        /// Draw function called from game. 
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch, List<Tile> redraw)
        {
            DrawTiles(spriteBatch, redraw);
        }
        /// <summary>
        /// Unloads the content
        /// </summary>
        public void Dispose()
        {
            content.Unload();
        }
        /// <summary>
        /// Checks if the tile is minable, if so change texture and make passable. Params x,y coords
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void mineTile(int x, int y)
        {
            if (tiles[x, y].getTileCollision() == TileCollision.MINEABLE)
            {
                tiles[x, y].setTexture(Content.Load<Texture2D>("dirt"));
                tiles[x, y].setPassable(TileCollision.PASSABLE);
                tiles[x, y].settype(TileType.DIRT);
            }
        }

        public bool generateLevel()
        {
            char[,] tiles;
            string[] result;
            int[] chances = {0,0,0,0,0};
            Random rnd = new Random();
            int rows = rnd.Next(9, 15);
            int cols = rnd.Next(11, 26);
            Config.applog.print(String.Format("rows: {0} cols: {1}", rows, cols), Config.LOGLEVEL);
            tiles = new char[rows, cols];
            result = new string[rows];
            int current;
            for (int i = 0; i < cols; i++)
            {
                result[0] += '#';
                tiles[0, i] = '#';
            }
            for (int y = 1; y < rows - 1; y++)
            {
                tiles[y, 0] = '#';
                for (int x = 1; x < cols - 1; x++)
                {
                    //add chances to add a spawnpoint as well
                    chances[0] = 10 + (tiles[y - 1, x - 1] == 'G' ? 16 : 0) + (tiles[y - 1, x] == 'G' ? 16 : 0)
                        + (tiles[y - 1, x + 1] == 'G' ? 16 : 0) + (tiles[y, x - 1] == 'G' ? 16 : 0);
                    chances[1] = 9 + (tiles[y - 1, x - 1] == 'R' ? 16 : 0) + (tiles[y - 1, x] == 'R' ? 16 : 0)
                        + (tiles[y - 1, x + 1] == 'R' ? 16 : 0) + (tiles[y, x - 1] == 'R' ? 16 : 0);
                    chances[2] = 8 + (tiles[y - 1, x - 1] == 'L' ? 16 : 0) + (tiles[y - 1, x] == 'L' ? 16 : 0)
                        + (tiles[y - 1, x + 1] == 'L' ? 16 : 0) + (tiles[y, x - 1] == 'L' ? 16 : 0);
                    chances[3] = 15 + (tiles[y - 1, x - 1] == 'D' ? 16 : 0) + (tiles[y - 1, x] == 'D' ? 16 : 0)
                        + (tiles[y - 1, x + 1] == 'D' ? 16 : 0) + (tiles[y, x - 1] == 'D' ? 16 : 0);
                    chances[4] = 8 + (tiles[y - 1, x - 1] == '#' ? 16 : 0) + (tiles[y - 1, x] == '#' ? 16 : 0)
                        + (tiles[y - 1, x + 1] == '#' ? 16 : 0) + (tiles[y, x - 1] == '#' ? 16 : 0);

                    current = rnd.Next(114);
                    Config.applog.print(string.Format("G: {0} R: {1} L: {2} D: {3} #: {4} next: {5}", chances[0], chances[1], chances[2], chances[3], chances[4], current));

                    if (current <= chances[0])
                    {
                        tiles[y, x] = 'G';
                    }
                    else if (current <= (chances[0] + chances[1]))
                    {
                        tiles[y, x] = 'R';
                    }
                    else if (current <= (chances[0] + chances[1] + chances[2]))
                    {
                        tiles[y, x] = 'L';
                    }
                    else if (current <= (chances[0] + chances[1] + chances[2] + chances[3]))
                    {
                        tiles[y, x] = 'D';
                    }
                    else
                    {
                        tiles[y, x] = '#';
                    }
                            
                }
                tiles[y, cols-1] = '#';
                    
                for (int i = 0; i < cols; i++)
                    result[y] += tiles[y, i];
                if (Config.DEBUG)
                {
                    Trace.WriteLine(result[y]);
                    Trace.WriteLine("\n");
                }
            }
            for (int i = 0; i < cols; i++)
            {
                result[rows-1] += '#';
                tiles[rows-1, i] = '#';
            }               
            File.WriteAllLines("Content/Levels/tmp.txt", result);
            return true;
        }

        public void spawn(GameTime gameTime)
        {
            //if (spawnPoint.Count == 1) { return; } //Spawn point busy
           
            int i = (int)new Random().Next(0, spawnPoint.Count - 1);
            if (spawnPoint[i].X != 0.0f && spawnPoint[i].Y != 0.0f)
            {
               // Trace.WriteLine(String.Format("X: {0}, Y: {1}", spawnPoint[i].X, spawnPoint[i].Y));
                tiles[(int)(spawnPoint[i].X), (int)(spawnPoint[i].Y)].getTileContains().AddCreature(new Creature(Content.Load<Texture2D>("blue_dot"), "blue_dot"));
              //  tiles[(int)(startPoint[i].X), (int)(startPoint[i].Y)].getTileContains().AddEnemy(new Enemy(Content.Load<Texture2D>("red_dot"), "red_dot"));
            }

           // if (startPoint.Count == 1) { return; } //Starting point busy
            i =  startPoint.Count-1;
            Config.applog.print(string.Format("Trying to add a red dot"), Config.LOGLEVEL);
            if(startPoint[i].X != 0.0f && startPoint[i].Y != 0.0f)
            {
                Config.applog.print(string.Format("\"adding a red dot at X:{0} and y: {1}\"", (int)startPoint[i].X, (int)startPoint[i].Y), Config.LOGLEVEL);
             tiles[(int)(startPoint[i].X), (int)(startPoint[i].Y)].getTileContains().AddEnemy(new Enemy(Content.Load<Texture2D>("red_dot"), "red_dot"));
            }
        }
    }
}
