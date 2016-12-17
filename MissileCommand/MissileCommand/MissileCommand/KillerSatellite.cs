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
    class KillerSatellite : GameObject
    {
        public const int maxVolleys = 4;
        public const int maxMissilesPerVolley = 2;
        public static Texture2D texture;
        public static Texture2D texture2;
        private Point size = new Point(28, 26);

        private float speed = 2.5f;
        private float dropDistance = 20f;
        private Vector2 position;
        private SpriteEffects spriteEffects;
        private int timeAlive = 0;

        // Left = false : Right = true
        bool direction;

        int volleysLaunched = 0;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y);
            }
        }

        int ticks = (int)(Game1.planeAlienSound.Duration.Milliseconds / 1000f * 60);

        public KillerSatellite(bool position)
        {
            this.position.X = position ? Game1.Instance.GraphicsDevice.Viewport.Width : 0;
            this.position.Y = 100;
            direction = !position;
            spriteEffects = position ? SpriteEffects.FlipHorizontally : new SpriteEffects();
        }

        public override void Update(List<GameObject> objects)
        {
            position.X += direction ? speed : -speed;
            if (position.X <= 0 || position.X + size.X >= Game1.Instance.GraphicsDevice.Viewport.Width && timeAlive > speed * size.X)
            {
                direction = !direction;
                position.Y += dropDistance;
                spriteEffects = direction ? new SpriteEffects() : SpriteEffects.FlipHorizontally;
            }

            if (Game1.rand.Next(300) == 0 && volleysLaunched < maxVolleys)
            {
                for (int i = Game1.rand.Next(maxMissilesPerVolley) + 1; i > 0; i--)
                    objects.Add(new Missile(position, Game1.Instance.GetTarget(), Game1.Instance.enemyMissileSpeed, false, Color.Red, true));

                volleysLaunched++;
            }

            objects.OfType<Battery>().ToList().ForEach(b =>
            {
                if (b.Bounds.Intersects(this.Bounds))
                {
                    this.Destroy();
                    b.ammo = 0;
                }
            });

            if (toDestroy)
            {
                objects.Add(new Fireball(position));
            }

            timeAlive++;

            if (timeAlive % ticks == 0)
                Game1.planeAlienSound.Play();
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(timeAlive % 3 == 0 ? texture2 : texture, Bounds, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, 0f, Vector2.Zero, spriteEffects, 1f);
        }
    }
}
