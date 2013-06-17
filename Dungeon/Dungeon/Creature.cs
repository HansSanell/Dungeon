using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dungeon
{
    class Creature
    {
        private static int counter = 0;
        int hp = 4;
        public String name;
        int dmg = 2;
        int def = 1;
        int haste;
        public bool moved;
        private Vector2 position;
        int id;
        public Texture2D texture;

        public Creature(Texture2D pTexture, String pName)
        {
            texture = pTexture;
            name = pName;
            moved = false;
            id = counter;
            counter++;
        }
        public Creature(Texture2D pTexture, String pName, bool pMoved)
        {
            texture = pTexture;
            name = pName;
            moved = pMoved;
            id = counter;
            counter++;
        }
        public Creature(Creature cret)
        {
            texture = cret.texture;
            name = cret.name;
            haste = cret.haste;
            def = cret.def;
            hp = cret.hp;
            dmg = cret.dmg;
            moved = cret.moved;
            id = cret.id;
        }
        public override bool Equals(object obj)
        {
            if (this.name == ((Creature)obj).name)
                return true;
            return false;

        }
         public bool battle(Tile[,] tiles, int x, int y)
        {
         /* extract all nearby creatures to a list
          * deal damage to all of them
          * all of them deal damage back. 
          * if hp is 0 remove object from list (creature). for enemy remove at false return
          * 
          */
            List<Enemy> enemies = tiles[x - 1, y].getTileContains().enemies;
            enemies.AddRange(tiles[x + 1, y].getTileContains().enemies);
            enemies.AddRange(tiles[x, y - 1].getTileContains().enemies);
            enemies.AddRange(tiles[x, y + 1].getTileContains().enemies);
            // all creatures are added to the list now, I hope
           // if(creatures.Count > 0)
            Config.applog.print(string.Format("Found {0} creatures adjecent to this tile {1} {2}", enemies.Count, x, y), Config.LOGLEVEL);
            foreach (Enemy enemy in enemies)
            {
                //damageDone = hp - Math.Max(0, (enemy.getDmg() - def));
                enemy.setHp(enemy.getHp() - Math.Max(0,(dmg - enemy.getDef())));
                hp = hp - Math.Max(0, (enemy.getDmg() - def));
                Config.applog.print(string.Format("Enemy took {0} dmg, got {1} hp left", Math.Max(0, dmg - enemy.getDef()), enemy.getHp()));
                
            }
            if (hp <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        
        public void move(Creature creature)
        {



        }
        public int getHp()
        {
            return hp;
        }
        public int getDef()
        {
            return def;
        }
        public void setHp(int php)
        {
            hp = php;
        }
        public int getDmg()
        {
            return dmg;
        }

    }
}
