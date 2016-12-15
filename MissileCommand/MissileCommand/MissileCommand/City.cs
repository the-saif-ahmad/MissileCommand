using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MissileCommand {
    class City : GameObject {

        public static Texture2D regularTexture;
        public static Texture2D destroyedTexture;

        public bool destroyed;

        public Point size = new Point(40, 20);

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(position.X, position.Y, size.X, size.Y);
            }
        }

        Point position;

        public City(int x, int y) {
            this.position = new Point(x, y);
            this.destroyed = false;
        }

        public override void Draw(SpriteBatch sb) {
            sb.Draw(destroyed ? destroyedTexture : regularTexture, Bounds, Color.White);
        }

        public override void Update(List<GameObject> objects) {
            
        }
    }
}
