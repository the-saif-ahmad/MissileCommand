using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MissileCommand {
    class Missile : GameObject {

        Point to;

        Vector2 position;

        float speed;

        Vector2 speedVector;

        static Point size = new Point(2, 2);

        public static Texture2D texture;

        TargetCrosshair crosshair;

        Trail trail;

        Color color;

        public bool friendly;
        public bool branched;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y);
            }
        }

        public Missile(Vector2 location, Point to, float speed, bool friendly, Color color) : this(location, to, speed, friendly, color, false) { }

        public Missile(Vector2 location, Point to, float speed, bool friendly, Color color, bool branched) {
            this.position = location;
            this.to = to;
            this.speed = speed;
            this.trail = null;
            this.color = color;
            this.friendly = friendly;
            this.branched = branched;

            if (friendly) this.crosshair = new TargetCrosshair(new Vector2(to.X, to.Y));
            else this.crosshair = new TargetCrosshair(new Vector2(-1000, -1000));

            double direction = Math.Atan2(to.X-location.X, to.Y-location.Y);
            speedVector = new Vector2((float)Math.Sin(direction) * speed, (float)Math.Cos(direction) * speed);
        }

        public override void Update(List<GameObject> objects) {
            bool destroy = Distance(position, to) <= speed;
            foreach (GameObject o in objects) {
                if (o is Fireball) {
                    var f = o as Fireball;
                    if (f.Contains(this.Bounds)) destroy = true;
                }
                if (o is Battery) {
                    var b = o as Battery;
                    if (b.Bounds.Intersects(this.Bounds)) destroy = true;
                }
            }

            if (!friendly && !branched && Game1.rand.Next(7222) == 0) {
                for (int i = Game1.rand.Next(4) + 1; i > 0; i--)
                    objects.Add(new Missile(position, Game1.Instance.GetTarget(), Game1.Instance.enemyMissileSpeed, false, Color.Red, true));

                branched = true;
            }

            if (destroy) {
                //explode
                Destroy();
                
                if (!friendly && Game1.Instance.state != GameState.GameOver) {
                    Game1.Instance.AddPoints(25);
                }

                objects.Add(new Fireball(position));
            } else {
                var next = position + speedVector;
                var normal = speedVector;
                normal.Normalize();
                trail = new Trail(position, trail);
                while ((next-position).Length()>1) {
                    trail = new Trail(position, trail);
                    position += normal;
                }
                position = next;
            }
        }

        private static float Distance(Vector2 a, Vector2 b) {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private static float Distance(Vector2 a, Point b) {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public override void Draw(SpriteBatch sb) {
            sb.Draw(texture, Bounds, Color.White);
            crosshair.Draw(sb);
            Trail t = trail;
            while (t != null) {
                sb.Draw(texture, t.Bounds, color);
                t = t.next;
            }
        }

        class Trail {
            public Vector2 position;
            public Trail next;
            public Rectangle Bounds { get { return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y); } }
            public Trail(Vector2 position, Trail next) {
                this.position = position;
                this.next = next;
            }

        }

    }
}
