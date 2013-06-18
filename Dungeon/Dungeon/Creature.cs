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
                return true;
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
