using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MissileCommand
{
    class TargetCrosshair : GameObject
    {
        private const int size = 15;
        public static Texture2D texture;

        private Vector2 position;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, size, size);
            }
        }

        public TargetCrosshair(Vector2 position)
        {
            this.position = new Vector2(position.X-size/2, position.Y-size/2);
        }

        public override void Update(List<GameObject> objects)
        {

        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(texture, Bounds, Color.White);
        }
    }
}
