using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MissileCommand
{

    public enum GameState
    {
        Splash, Playing, TransitionToScoring, Scoring, GameOver
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public List<GameObject> objects;

        Texture2D ground;
        Rectangle groundSize = new Rectangle(0, 0, 800, 480);

        Texture2D crosshairText;
        Rectangle crosshairRect;

        Texture2D splash;
        Texture2D gameOverSplash;

        SpriteFont font;

        public int stageNumber;
        int numEnemyMissiles;
        int numBombsAndKS;
        int numSmartBombs;
        public float enemyMissileSpeed = 0.5f;

        public int score = 0;

        int citiesAtStart = 6;

        public GameState state;

        public static Random rand = new Random();

        public static Game1 Instance;

        int timeScoring = 0;

        int ammoLeftover = -1;
        int citiesLeftover;

        public static SoundEffect counting;
        public static SoundEffect emptyBatteryLaunch;
        public static SoundEffect levelStart;
        public static SoundEffect missileExplosion;
        public static SoundEffect missileLaunch;
        public static SoundEffect outOfMissiles;
        public static SoundEffect planeAlienSound;

        public bool testSpawn = true;

        private bool isFullScreen = false;

        public Game1()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            KB.onPress(Keys.Escape, this.Exit);
            objects = new List<GameObject>();
            this.state = GameState.Splash;

            if (isFullScreen)
            {
                graphics.PreferredBackBufferHeight = 1080;
                graphics.PreferredBackBufferWidth = 1920;
                graphics.IsFullScreen = true;
            }
            else
            {
                graphics.PreferredBackBufferHeight = 600;
                graphics.PreferredBackBufferWidth = 480;
                graphics.IsFullScreen = false;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            objects.Add(new Battery(new Point(50, 410), Keys.Z, 3f));
            objects.Add(new Battery(new Point(371, 410), Keys.X, 4.5f));
            objects.Add(new Battery(new Point(726, 410), Keys.C, 3f));

            objects.Add(new City(130, 418));
            objects.Add(new City(195, 423));
            objects.Add(new City(280, 430));
            objects.Add(new City(445, 425));
            objects.Add(new City(545, 415));
            objects.Add(new City(625, 423));

            stageNumber = 1;
            numEnemyMissiles = 3 * stageNumber + 3;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ground = Content.Load<Texture2D>("Ground");
            Battery.ammoTex = Content.Load<Texture2D>("Ammo");
            TargetCrosshair.texture = Content.Load<Texture2D>("TargetCrosshair");
            crosshairText = this.Content.Load<Texture2D>("crosshair");
            splash = this.Content.Load<Texture2D>("SplashScreen");
            Missile.texture = Content.Load<Texture2D>("Square");
            Fireball.white = Content.Load<Texture2D>("FireballWhite");
            Fireball.blue = Content.Load<Texture2D>("FireballBlue");
            Fireball.pink = Content.Load<Texture2D>("FireballPink");
            City.regularTexture = Content.Load<Texture2D>("City");
            City.destroyedTexture = Content.Load<Texture2D>("CityDestroyed");
            Battery.outIndicator = Content.Load<Texture2D>("Out");
            Battery.lowIndicator = Content.Load<Texture2D>("Low");
            gameOverSplash = Content.Load<Texture2D>("GameOver");
            font = Content.Load<SpriteFont>("Font");
            Bomber.texture = Content.Load<Texture2D>("Plane");
            KillerSatellite.texture = Content.Load<Texture2D>("KillerSatellite");
            KillerSatellite.texture2 = Content.Load<Texture2D>("altKillerSatellite");
            SmartBomb.texture = Content.Load<Texture2D>("SmartBomb");
            SmartBomb.texture2 = Content.Load<Texture2D>("altSmartBomb");

            counting = Content.Load<SoundEffect>("missileCounting");
            emptyBatteryLaunch = Content.Load<SoundEffect>("emptyBatteryLaunch");
            levelStart = Content.Load<SoundEffect>("levelStart");
            missileExplosion = Content.Load<SoundEffect>("missileExplosion");
            missileLaunch = Content.Load<SoundEffect>("missileLaunch");
            outOfMissiles = Content.Load<SoundEffect>("outOfMissiles");
            planeAlienSound = Content.Load<SoundEffect>("planeAlienSound");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            Utilities.update();
            MouseState mouse = Mouse.GetState();

            crosshairRect = new Rectangle(mouse.X - 7, mouse.Y - 7, 15, 15);

            if (this.state == GameState.Splash)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space)) this.state = GameState.Scoring;
            }

            if (this.state == GameState.Playing)
            {
                //end game if no cities
                if (objects.OfType<City>().Where(c => !c.destroyed).Count() == 0) this.state = GameState.GameOver;

                if (state != GameState.GameOver && numEnemyMissiles > 0 && rand.Next(90) == 0)
                {
                    objects.Add(new Missile(new Vector2(rand.Next(40, 760), 0), GetTarget(), enemyMissileSpeed * Math.Max(stageNumber / 3f, 1), false, Color.Red));
                    numEnemyMissiles--;
                }

                if (state != GameState.GameOver && numBombsAndKS > 0 && rand.Next(90) == 0)
                {
                    bool direction = rand.Next(1) == 0;

                    switch (rand.Next(2))
                    {
                        case 0:
                            objects.Add(new Bomber(direction));
                            break;

                        case 1:
                            objects.Add(new KillerSatellite(direction));
                            break;
                    }

                    numBombsAndKS--;
                }

                if (state != GameState.GameOver && numSmartBombs > 0 && rand.Next(90) == 0)
                {
                    objects.Add(new SmartBomb(rand.Next(GraphicsDevice.Viewport.Width), GetTarget()));
                    numSmartBombs--;
                }

                objects.ForEach(o => o.Update(objects));
                objects = objects.Where(o => !o.toDestroy).ToList();

                //transition to transition stage
                if ((numEnemyMissiles <= 0 &&
                    numBombsAndKS <= 0 &&
                    numSmartBombs <= 0 &&
                    objects.OfType<Missile>().ToList().Count == 0 &&
                    objects.OfType<Fireball>().ToList().Count == 0 &&
                    objects.OfType<Bomber>().ToList().Count == 0 &&
                    objects.OfType<KillerSatellite>().ToList().Count == 0 &&
                    objects.OfType<SmartBomb>().ToList().Count == 0) || //no more enemies this stage
                    (objects.OfType<City>().Where(c => c.destroyed).ToList().Count >= 3))
                { //3 cities destroyed this stage
                    this.state = GameState.TransitionToScoring;
                }
                if (this.objects.Where(o => (o is City && !(o as City).destroyed)).ToList().Count == 0) this.state = GameState.GameOver;

            }

            if (this.state == GameState.TransitionToScoring)
            {
                this.state = GameState.Scoring;
                stageNumber++;
                numEnemyMissiles = 3 * stageNumber + 3;
                numBombsAndKS = stageNumber > 4 ? stageNumber - 4 : 0;
                numSmartBombs = stageNumber > 5 ? (stageNumber - 5) / 2 : 0;
                this.timeScoring = 0;
                this.objects = this.objects.Where(o => (o is City && !(o as City).destroyed) || (o is Battery)).ToList();
                //add cities remaining to score
                citiesLeftover = objects.OfType<City>().ToList().Count;
                
                citiesAtStart = objects.OfType<City>().ToList().Count;
                //add remaining ammo to score and reload
                ammoLeftover = 0;
                objects.OfType<Battery>().ToList().ForEach(b =>
                {
                    ammoLeftover += b.ammo;
                    b.ammo = 10;
                });

                lastAmmo = 0;
                lastCity = 0;
            }

            if (this.state == GameState.Scoring)
            {
                //pause to let the player recuperate
                timeScoring++;

                if (timeScoring == 240)
                {
                    AddPoints(25 * (ammoLeftover > 0 ? ammoLeftover : 0));
                    AddPoints(100 * citiesLeftover);
                    this.state = GameState.Playing;
                    levelStart.Play();
                }
            }

            if (this.state == GameState.GameOver)
            {
                objects.ForEach(o => o.Update(objects));
                objects = objects.Where(o => !o.toDestroy).ToList();
            }

            base.Update(gameTime);
        }

        int lastAmmo = 0;
        int lastCity = 0;

        protected override void Draw(GameTime gameTime)
        {

            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, 800, 480);
            GraphicsDevice.SetRenderTarget(target);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            if (this.state == GameState.Splash)
            {
                spriteBatch.Draw(splash, groundSize, Color.White);
            }
            else
            {
                //DRAWING LOGIC HERE
                spriteBatch.Draw(ground, groundSize, Color.White);
                spriteBatch.DrawString(font, $"Score: {score}", new Vector2(0, -10), Color.LimeGreen);
                if (state == GameState.Scoring)
                {
                    string s = $"Stage {stageNumber}";
                    var size = font.MeasureString(s);
                    spriteBatch.DrawString(font, s, new Vector2(800 / 2 - size.X / 2, 70 - size.Y / 2), Color.LimeGreen);
                }

                if (state == GameState.Scoring && ammoLeftover >= 0)
                {
                    string s = "Bonus Points";
                    var size = font.MeasureString(s);
                    spriteBatch.DrawString(font, s, new Vector2(800 / 2 - size.X / 2, 140 - size.Y / 2), Color.LimeGreen);

                    int forAmmo = ammoLeftover > 0 ? 100 / ammoLeftover : 0;
                    int forCities = 100 / citiesLeftover;
                    int ammo = Math.Min(timeScoring / forAmmo, ammoLeftover);
                    int cities = timeScoring > forAmmo * ammoLeftover ? Math.Min((timeScoring - (forAmmo * ammoLeftover)) / forCities, citiesLeftover) : 0;

                    if (lastAmmo != ammo) counting.Play();
                    lastAmmo = ammo;

                    var ammoScore = ammo * 25 * GetScoringMultiplier(stageNumber) + "";
                    size = font.MeasureString(ammoScore);
                    var ammoPos = new Vector2(800 / 3 - size.X, 420 / 2 - size.Y / 2);
                    spriteBatch.DrawString(font, ammoScore, ammoPos, Color.LimeGreen);
                    var ammoSize = new Point((int)(Battery.ammoSize.X * Battery.ammoScale), (int)(Battery.ammoSize.Y * Battery.ammoScale));
                    for (int i = 0; i < ammo; i++)
                    {
                        spriteBatch.Draw(Battery.ammoTex, new Rectangle(((int)ammoPos.X + (int)size.X + 30) + (i * (ammoSize.X + 3)), (int)ammoPos.Y + 20, ammoSize.X, ammoSize.Y), Color.White);
                    }

                    var citiesScore = cities * 100 * GetScoringMultiplier(stageNumber) + "";// + " " + forAmmo+" "+timeScoring;
                    size = font.MeasureString(citiesScore);
                    var citiesPos = new Vector2(800 / 3 - size.X, (420 / 2 - size.Y / 2) + 60);
                    spriteBatch.DrawString(font, citiesScore, citiesPos, Color.LimeGreen);
                    var citySize = new Point(40, 20);
                    for (int i = 0; i < cities; i++)
                    {
                        spriteBatch.Draw(City.regularTexture, new Rectangle(((int)citiesPos.X + (int)size.X + 30) + (i * (citySize.X + 5)), (int)citiesPos.Y + 15, citySize.X, citySize.Y), Color.White);
                    }

                    if (lastCity != cities) counting.Play();
                    lastCity = cities;
                }

                spriteBatch.Draw(crosshairText, crosshairRect, Color.White);
                objects.ForEach(o => o.Draw(spriteBatch));
                if (state == GameState.GameOver) spriteBatch.Draw(gameOverSplash, new Vector2(0, 0), Color.White);
            }
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            //render target to back buffer
            spriteBatch.Begin();
            // Console.WriteLine(GraphicsDevice.DisplayMode.Width + " " + GraphicsDevice.DisplayMode.Height);
            spriteBatch.Draw(target, new Rectangle(0, 0, GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public Point GetTarget()
        {
            List<GameObject> targets = objects.Where(o => (o is City && !(o as City).destroyed) || (o is Battery && (o as Battery).ammo > 0)).ToList();
            if (targets.Count == 0) return new Point(800 / 2, 480 / 2);
            GameObject t = targets[rand.Next(targets.Count)];
            if (t is City)
            {
                var c = t as City;
                return new Point(c.Bounds.Center.X, c.Bounds.Y);
            }
            if (t is Battery)
            {
                var b = t as Battery;
                return new Point(b.Bounds.Center.X, b.Bounds.Y);
            }
            return new Point(800, 480);
        }

        public void AddPoints(int points)
        {
            this.score += points * GetScoringMultiplier(stageNumber);
        }

        public static int GetScoringMultiplier(int stage)
        {
            return stage < 11 ? (int)((stage + 1) / 2) : 6;
        }
    }
}
