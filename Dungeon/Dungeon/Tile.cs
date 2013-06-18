using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dungeon
{
    enum TileType
    {
        GOLD=0,
        FLOORED=1,
        RUBBLE=2,
        STONE=3,
        DIRT=4,
        LAVA=5,
        MARKED=6,
    }
    enum TileCollision
    {
        IMPASSABLE=0,
        PASSABLE=1,
        MINEABLE=2,
        TAKEN=3,
    }

    class TileContains
    {
        public List<Enemy> enemies;
        public List<Creature> creatures;

        public TileContains()
        {
            enemies = new List<Enemy>();
            creatures = new List<Creature>();
        }
        public void AddCreature(Creature cret)
        {
            creatures.Add(cret);
        }
        public Creature RemoveCreature(Creature cret)
        {
            
            //doesn't remove the creature as it should..... 
            creatures.Remove(cret);
            return cret;
           
        }
        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
        }
        public Enemy RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
            Config.applog.print("remove enemy", Config.LOGLEVEL);
            return enemy;
        }
        
    }
    class Tile
    {
        private TileType type;
        public Vector2 size = new Vector2(Game1.SPRITE_WIDTH, Game1.SPRITE_HEIGHT);
        private TileContains contains;
        private TileCollision passable;
        private Texture2D texture;
        private Vector2 position;
        private bool drawn;
        public Tile(int pType)
        {
            type = (TileType)pType;
            contains = new TileContains();
            passable = 0 ;
            texture = null;
            drawn = false;
        }
        public Tile(TileType pType, Texture2D pTexture, TileCollision pPassable, Vector2 pPosition, bool pDrawn)
        {
            type = pType;
            contains = new TileContains();
            texture = pTexture;
            passable = pPassable;
            position = pPosition * size;
            drawn = pDrawn;
        }
        public Texture2D getTexture()
        {
            return texture;
        }
        public void setTexture(Texture2D ptexture)
        {
            texture = ptexture;
        }
        public void setDrawn(bool pDrawn)
        {
            drawn = pDrawn;
        }
        public bool getDrawn()
        {
            return drawn;
        }
        public Vector2 getPosition()
        {
            return position;
        }
        public TileCollision getTileCollision()
        {
            return passable;
        }
        public void settype(TileType tiletype)
        {
            type = tiletype;
        }
        public void setPassable(TileCollision pPassable)
        {
            passable = pPassable;
        }
        public TileContains getTileContains()
        {
            return contains;
        }
        public void setTaken(TileCollision pTaken)
        {
            passable = pTaken;
        }
    }
}
