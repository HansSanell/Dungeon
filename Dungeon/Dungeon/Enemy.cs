using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dungeon
{

    class Enemy
    {
        int hp = 5;
        public String name;
        int dmg = 1;
        int def = 1;
        int haste;
        public Texture2D texture;
        public bool moved;
        
        private Vector2 position;
        public Enemy()
        {

        }
        public Enemy(Texture2D pTexture, String pName, bool pMoved)
        {
            texture = pTexture;
            name = pName;
            moved = pMoved;
            
        }
        public Enemy(Texture2D pTexture, String pName)
        {
            texture = pTexture;
            name = pName;
        }
        public Enemy(Enemy cret)
        {
            texture = cret.texture;
            name = cret.name;
            haste = cret.haste;
            def = cret.def;
            hp = cret.hp;
            dmg = cret.dmg;
            moved = cret.moved;
         
        }
        
        private void resolve()
        {

        }
        public int getHp()
        {
            return hp;
        }
        public int getDmg()
        {
            return dmg;
        }
        public void setHp(int php)
        {
            hp = php;
        }
        public int getDef()
        {
            return def;
        }
    }
}
