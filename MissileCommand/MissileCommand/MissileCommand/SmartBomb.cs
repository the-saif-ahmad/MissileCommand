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
    class SmartBomb : GameObject
    {
        public static Texture2D texture;
        public static Texture2D texture2;

        Point target;

        Vector2 position;
        List<Vector2> advancedPositions;
        float speed = 0.8f;
        bool dodged = false;

        Vector2 speedVector;

        Point size = new Point(18, 16);

        int ticks = (int)(Game1.planeAlienSound.Duration.Milliseconds / 1000f * 60);

        public Rectangle Bounds {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y);
            }
        }

        Func<int, Rectangle> TestBounds;

        int timeAlive = 0;

        public SmartBomb(int xPos, Point target)
        {
            this.target = target;
            position = new Vector2(xPos, -size.Y);
            advancedPositions = new List<Vector2>();
            NewTarget(target);

            TestBounds = index => new Rectangle((int)advancedPositions[index].X, (int)advancedPositions[index].Y, size.X, size.Y);
        }

        public override void Update(List<GameObject> objects)
        {
            advancedPositions.Clear();
            bool destroy = Distance(position, target) <= speed;
            position += speedVector;

            for (int i = 1; i < 20; i++)
            {
                advancedPositions.Add(position + speedVector * i);
            }

            objects.OfType<Fireball>().ToList().ForEach(f =>
            {
                for (int i = 0; i < advancedPositions.Count; i++)
                {
                    if (f.Contains(TestBounds(i)))
                    {
                        float direction = this.Bounds.Center.X - f.Bounds.Center.X;
                        float multiplier = 0.9f; // <- How easily it dodges fireballs; ^multiplier = ^dodging

                        if (this.Bounds.Center.X > f.Bounds.Center.X + 10)
                        {
                            speedVector.X = (position.X - advancedPositions[i].X) / direction * multiplier;
                        }
                        else
                        {
                            speedVector.X = (advancedPositions[i].X - position.X) / direction * multiplier;
                        }
                        dodged = true;
                    }
                    else if (dodged)
                    {
                        NewTarget(Game1.Instance.GetTarget());
                        dodged = false;
                    }
                }
            });

            if (destroy)
            {
                //explode
                Destroy();

                objects.Add(new Fireball(position));
            }
            else
            {
                position += speedVector;
            }

            if (timeAlive % ticks == 0)
                Game1.planeAlienSound.Play();

            timeAlive++;
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(timeAlive % 5 == 0 ? texture2 : texture, this.Bounds, Color.White);
        }

        private void NewTarget(Point target)
        {
            this.target = target;
            double direction = Math.Atan2(target.X - position.X, target.Y - position.Y);
            speedVector = new Vector2((float)Math.Sin(direction) * speed, (float)Math.Cos(direction) * speed);
        }

        private static float Distance(Vector2 a, Point b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}
