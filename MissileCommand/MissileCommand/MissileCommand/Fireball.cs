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
    class Fireball : GameObject
    {
        public static Texture2D white;
        public static Texture2D blue;
        public static Texture2D pink;
        private const float maxSize = 50f;
        private const float rate = 0.4f;

        Vector2 position;
        private float size;
        private bool growing;

        int textureNumber = 0;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)(position.X - size / 2), (int)(position.Y - size / 2), (int)size, (int)size);
            }
        }

        public Fireball(Vector2 position)
        {
            this.position = position;
            size = 0.01f;
            growing = true;
            Game1.missileExplosion.Play();
        }

        public bool Contains(Rectangle rect)
        {
            if (rect.Contains(this.Bounds.Center)) return true;
            Point upperLeft = new Point(rect.X, rect.Y);
            Point upperRight = new Point(rect.X + rect.Width, rect.Y);
            Point lowerLeft = new Point(rect.X, rect.Y + rect.Height);
            Point lowerRight = new Point(rect.X + rect.Width, rect.Y + rect.Height);

            Point center = Bounds.Center;

            if (Distance(upperLeft, center) < size / 2) return true;
            if (Distance(upperRight, center) < size / 2) return true;
            if (Distance(lowerLeft, center) < size / 2) return true;
            if (Distance(lowerRight, center) < size / 2) return true;

            return false;
        }

        public static float Distance(Point a, Point b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(textureNumber < 2 ? white : textureNumber < 4 ? blue : pink, Bounds, Color.White);
            textureNumber = textureNumber == 6 ? 0 : textureNumber + 1;
        }

        public override void Update(List<GameObject> objects)
        {
            size += growing ? rate : -rate;
            if (size >= maxSize) growing = false;
            if (size <= 0) Destroy();
            objects.OfType<Battery>().ToList().ForEach(b => b.ammo = this.Bounds.Intersects(b.Bounds) ? 0 : b.ammo);
            objects.OfType<City>().ToList().ForEach(c => { if (this.Bounds.Intersects(c.Bounds)) c.destroyed = true; });
            objects.OfType<Bomber>().ToList().ForEach(p => 
            {
                if (this.Bounds.Intersects(p.Bounds))
                {
                    p.Destroy();
                    Game1.Instance.AddPoints(100);
                }
            });
            objects.OfType<KillerSatellite>().ToList().ForEach(k =>
            {
                if (this.Bounds.Intersects(k.Bounds))
                {
                    k.Destroy();
                    Game1.Instance.AddPoints(100);
                }
            });
            objects.OfType<SmartBomb>().ToList().ForEach(s =>
            {
                if (this.Contains(s.Bounds))
                {
                    s.Destroy();
                    Game1.Instance.AddPoints(125);
                }
            });
        }
    }
}
