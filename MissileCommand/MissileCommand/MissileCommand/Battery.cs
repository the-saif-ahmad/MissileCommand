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
    class Battery : GameObject
    {

        Point position;

        static Point size = new Point(50, 50);

        public static Texture2D ammoTex;
        public static Texture2D lowIndicator;
        public static Texture2D outIndicator;

        public bool outOfMissiles = false;

        static Point[] placements = new Point[] {
            new Point(9, 0),
            new Point(6, 3),
            new Point(12, 3),
            new Point(3, 6),
            new Point(9, 6),
            new Point(15, 6),
            new Point(0, 9),
            new Point(6, 9),
            new Point(12, 9),
            new Point(18, 9),
        };

        public static Point ammoSize = new Point(3, 5);

        public int ammo = 10;
        public float speed;

        public const float ammoScale = 2.5f;

        Action fireAction;
        Keys fireKey;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y);
            }
        }

        public Rectangle Indicator
        {
            get
            {
                return new Rectangle(position.X, position.Y + size.Y * 3 / 4, 50, 20);
            }
        }

        public Battery(Point position, Keys fireKey, float speed)
        {
            this.position = position;
            this.fireKey = fireKey;
            fireAction = this.Fire;
            KB.onPress(fireKey, fireAction);
            this.speed = speed;
        }

        public override void Destroy()
        {
            this.toDestroy = true;
            KB.removeOnPress(fireKey, fireAction);
        }

        public override void Update(List<GameObject> objects)
        {
            if (ammo <= 0 && !outOfMissiles)
            {
                outOfMissiles = true;
                Game1.outOfMissiles.Play(0.5f, 0f, 0f);
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < ammo; i++)
            {
                var pos = placements[i];
                sb.Draw(ammoTex, new Rectangle(position.X + (int)(pos.X * ammoScale), position.Y + (int)(pos.Y * ammoScale), (int)(ammoSize.X * ammoScale), (int)(ammoSize.Y * ammoScale)), Color.White);
            }

            if (ammo <= 0) sb.Draw(outIndicator, Indicator, Color.White);
            else if (ammo <= 3) sb.Draw(lowIndicator, Indicator, Color.White);
        }

        public void Fire()
        {

            /*
             *Can fire if:
             *battery has ammo
             *less than 3 friendly missiles on the screen
             *game is in play state
            */

            if (ammo > 0 && Game1.Instance.state == GameState.Playing && Game1.Instance.objects.Where(o => o is Missile && (o as Missile).friendly).ToList().Count < 3) {
                ammo--;
                var mouse = Mouse.GetState();
                var pos = new Point((int)(mouse.X * (800f / Game1.Instance.GraphicsDevice.DisplayMode.Width)), (int)(mouse.Y * (480f / Game1.Instance.GraphicsDevice.DisplayMode.Height)));
                Game1.Instance.objects.Add(new Missile(new Vector2(this.position.X + (size.X / 2), this.position.Y - 2), new Point(pos.X, pos.Y), speed, true, Color.LimeGreen));
                Game1.missileLaunch.Play();
            }
            else if (ammo <= 0) {
                Game1.emptyBatteryLaunch.Play();
            }
        }


    }
}
