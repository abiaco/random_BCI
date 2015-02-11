using System;
using EEG_Example_1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using MapGameLibrary;
using Emotiv;
//using System.IO;
using System.Threading;
using System.Timers;
using System.Reflection;
using System.Collections.Generic;
using ILNumerics;
using System.Threading.Tasks;
using OxyPlot;

namespace MazeShooter
{
    public class MazeShooterGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SoundEffect shootEffect;
        SoundEffect mineEffect;
        SoundEffect enemyDeadEffect;
        SoundEffect shipDeadEffect;
        Song background2;
        EEG_Logger log; //EEG Logger
        System.Timers.Timer timer_up;
        System.Timers.Timer timer_down;
        System.Timers.Timer timer_left;
        System.Timers.Timer timer_right;
        System.Timers.Timer data_read;
        int lev;
        int O1_count;
        int nocounts;
        ILNumerics.ILArray<double> O1;
        public static Input input;

        Camera camera1, camera2;
        FreeCamera freeCamera;
        Vector3 camera2Offset = new Vector3(5, 10, 5);
        int currentCamera;
        bool freeCam;

        Color up_color, down_color, left_color, right_color;

        Level level;

        // Debug info
        Grid grid;
        XYZ xyz;
        Paths paths;
        Path path;

        bool drawGrid = false;
        bool drawXYZ = false;
        bool drawPaths = false;
        bool drawPath = false;
        bool drawLevel = true;
        bool textured = true;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        String[] mapNames = new String[] { "level2", "level1"};

        public MazeShooterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.IsMouseVisible = true;
        }

        private void OnTimedEventUp(object source, ElapsedEventArgs e)
        {
            up_color = Color.White;
        }

        private void OnTimedEventDown(object source, ElapsedEventArgs e)
        {
            down_color = Color.White;
        }

        private void OnTimedEventLeft(object source, ElapsedEventArgs e)
        {
            left_color = Color.White;
        }

        private void OnTimedEventRight(object source, ElapsedEventArgs e)
        {
            right_color = Color.White;
        }

        

        public void start_timers()
        {
            data_read = new System.Timers.Timer();
            timer_up = new System.Timers.Timer();
            timer_down = new System.Timers.Timer();
            timer_left = new System.Timers.Timer();
            timer_right = new System.Timers.Timer();
            data_read.Elapsed += new ElapsedEventHandler(OnTimedEventData);
            timer_up.Elapsed += new ElapsedEventHandler(OnTimedEventUp);
            timer_down.Elapsed += new ElapsedEventHandler(OnTimedEventDown);
            timer_left.Elapsed += new ElapsedEventHandler(OnTimedEventLeft);
            timer_right.Elapsed += new ElapsedEventHandler(OnTimedEventRight);
            data_read.Interval = 4000;
            timer_up.Interval = 76; //13 hz
            timer_down.Interval = 58; //17 hz
            timer_left.Interval = 43; //23 hz
            timer_right.Interval = 34; //29 hz
            timer_up.Enabled = true;
            timer_down.Enabled = true;
            timer_left.Enabled = true;
            timer_right.Enabled = true;
            data_read.Start();
            timer_up.Start();
            timer_down.Start();
            timer_left.Start();
            timer_right.Start();
        }
        protected override void Initialize()
        {
            freeCam = false;
            currentCamera = 1;
            lev = 0;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            up_color = Color.Black;
            down_color = Color.Black;
            left_color = Color.Black;
            right_color = Color.Black;
            log = new EEG_Logger();
            Task.Factory.StartNew(() => start_timers());
            O1 = new ILArray<double>[2001];
            O1_count = 0;
            base.Initialize();
            nocounts = 0;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Random rnd = new Random();
            input = new Input();
            font = Content.Load<SpriteFont>("fonts/font");
            shootEffect = Content.Load<SoundEffect>("sounds/fire");
            mineEffect = Content.Load<SoundEffect>("sounds/bangSmall");
            shipDeadEffect = Content.Load<SoundEffect>("sounds/bangLarge");
            enemyDeadEffect = Content.Load<SoundEffect>("sounds/bangMedium");
            background2 = Content.Load<Song>("sounds/Enter Sandman");


            level = new Level(Content.Load<Map>("maps/" + mapNames[lev]), Content, GraphicsDevice, shootEffect, mineEffect, shipDeadEffect, enemyDeadEffect);

            MediaPlayer.IsRepeating = true;
            //MediaPlayer.Play(background2);
            freeCamera = new FreeCamera(GraphicsDevice, new Vector3(level.Columns / 2, 10, level.Rows / 2), new Vector3(0, 5, 0), Vector3.Up, MathHelper.PiOver4, 0.1f, 1000f);

            Matrix shipWorld = level.ship.World;
            Vector3 look = shipWorld.Forward;
            look.Normalize();

            camera1 = new Camera(GraphicsDevice, shipWorld.Translation + (1.2f * -look) + (0.3f * shipWorld.Up), shipWorld.Translation + shipWorld.Forward, shipWorld.Up, MathHelper.PiOver4, 0.1f, 1000f);
            camera2 = new Camera(GraphicsDevice, shipWorld.Translation + camera2Offset, shipWorld.Translation, Vector3.Up, MathHelper.ToRadians(40), 0.1f, 100f);

            if(drawGrid)
                grid = new Grid(level.Rows, level.Columns, GraphicsDevice);
            if (drawXYZ)
                xyz = new XYZ(level.Rows, level.Columns, GraphicsDevice);
            if (drawPaths)
                paths = new Paths(level.Rows, level.Columns, level.graph.GetLinks(), GraphicsDevice);
            if (drawPath)
                path = new Path(level.getEnemyPath(), GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected void readlog()
        {
            Dictionary<string, LinkedList<double>> d = log.Run();
            List<string> list = new List<string>(d.Keys);
            //O1 = new ILArray<double>[0];
            if (d.Keys.Count > 0)
            {
                foreach (string l in d.Keys)
                {
                    O1 = new ILArray<double>[d[l].Count];
                    int i = 0;
                   
                    foreach (double db in d[l])
                    {
                        O1[i] = db;
                        i++;
                    }
                    //Task.Factory.StartNew(() => System.Console.WriteLine(ILNumerics.ILMath.fft(O1)));
                    ILNumerics.ILRetArray<ILNumerics.complex> o;
                    o = ILNumerics.ILMath.fft(O1);
                    LinkedList<double> ds = extract_freq(o);
                    double max = get_max(ds);
                    double avg = average(ds);
                    Console.WriteLine("LIST: ");
                    //print_list(ds);
                    Console.WriteLine("max: " + max + " avg: " + avg);
                    if (max - avg > 13000)
                    {
                        Task task = new Task(new Action(() => level.ship.Accerlerate()));
                        task.Start();
                        task.Wait(2000);
                    }
                    //Console.Write("list is: ");
                    //Task.Factory.StartNew(() => print_list(ds));
                }
                
            }
            
        }

        protected double average(LinkedList<double> d)
        {
            double sum = 0;
            foreach (double db in d)
            {
                sum += db;
            }
            return sum / d.Count;
        }

        protected void print_list(LinkedList<double> ds)
        {
            Console.WriteLine("LIST: ");
            foreach (double db in ds)
            {
                Console.Write(db + " ");
            }
            Console.WriteLine("\nNumber of Elements: " + O1_count);
        }


        protected double get_max(LinkedList<double> d)
        {
            double max = 0;
            foreach (double db in d){
                if(db > max)max = db;
            }
            return max;
        }

        protected double average(ILNumerics.ILRetArray<ILNumerics.complex> o)
        {
            double sum=0, count = 0;
            foreach (ILNumerics.complex a in o)
            {
                count++;
                sum += a.real;
            }
            return sum/count;
        }


        protected LinkedList<double> extract_freq(ILNumerics.ILRetArray<ILNumerics.complex> o)
        {
            if (o.Length > 0)
            {
                double x = average(o);
                LinkedList<double> res = new LinkedList<double>();
                foreach (ILNumerics.complex a in o)
                {
                    double n = Math.Round(Math.Sqrt(a.real * a.real + a.imag * a.imag) * 128 / o.Length, 2);
                    if (!double.IsInfinity(n))
                        res.AddLast(Math.Round(Math.Sqrt(a.real * a.real + a.imag * a.imag) * 128 / o.Length, 2));
                    else
                        res.AddLast(0);
                }

                return res;
            }
            return null;
        }

        
        protected void logO()
        {
           Dictionary<string, LinkedList<double>> d = log.Run();
           if (d.Keys.Count > 0)
           {
               List<string> list = new List<string>(d.Keys);
               double mn = average(d[list[0]]);
               foreach (double db in d[list[0]])
               {
                   if (O1_count < 2000)
                   {
                       O1[O1_count] = db - mn;
                       O1_count++;
                   }
               }
           }
           
        }

        int sum_count_13 = 0;
        int sum_count_17 = 0;
        int sum_count_23 = 0;
        int sum_count_29 = 0;

        

        protected int scan_list(LinkedList<double> list)
        {
            int move = 0;
            int count_13 = 0;
            int count_17 = 0;
            int count_23 = 0;
            int count_29 = 0;

            int avg_count_13 = 0;
            int avg_count_17 = 0;
            int avg_count_23 = 0;
            int avg_count_29 = 0;

            nocounts++;
            foreach (double db in list)
            {
                if (db > 11 && db < 15) count_13++;
                if (db > 15 && db < 19) count_17++;
                if (db > 21 && db < 25) count_23++;
                if (db > 27 && db < 31) count_29++;
            }
            sum_count_13 += count_13;
            sum_count_17 += count_17;
            sum_count_23 += count_23;
            sum_count_29 += count_29;

            avg_count_13 = (int) sum_count_13 / nocounts;
            avg_count_17 = (int) sum_count_17 / nocounts;
            avg_count_23 = (int) sum_count_23 / nocounts;
            avg_count_29 = (int) sum_count_29 / nocounts;
            /*
            if (count_13 > count_17 && count_13 > count_23 && count_13 > count_29) move = 1;
            if (count_17 > count_13 && count_17 > count_23 && count_17 > count_29) move = 2;
            if (count_23 > count_13 && count_23 > count_17 && count_23 > count_29) move = 3;
            if (count_29 > count_13 && count_29 > count_17 && count_29 > count_23) move = 4;
             */
            Console.Write("13s: " + count_13 + " 17s: " + count_17 + " 23s: " + count_23 + " 29s: " + count_29 + "\n");

            if (count_29 > 75) move = 4;
            else if (count_23 > 150) move = 3;
            else if (count_17 > 190) move = 2;
            else if (count_13 > 210) move = 1;
            return move;
        }


        private void OnTimedEventData(object source, ElapsedEventArgs e)
        {
            ILNumerics.ILRetArray<ILNumerics.complex> o = ILNumerics.ILMath.fft(O1);
            if (O1.IsEmpty)
            {
                return;
            }
            LinkedList<double> ds = extract_freq(o);
            //print_list(ds);
            Task.Factory.StartNew(() => plot(ds));
            int move = scan_list(ds);
            switch(move){
                case 1:
                    level.ship.Accerlerate();
                    break;
                case 2:
                    level.ShipShoot();
                    break;
                case 3:
                    level.ship.RotateShipLeft(90);
                    break;
                case 4:
                    level.ship.RotateShipRight(90);
                    break;
                default:
                    break;
            }
            
            O1 = new ILArray<double>[2000];
            O1_count = 0;
        }

        protected void plot(LinkedList<double> ds)
        {
            MLApp.MLApp matlab = new MLApp.MLApp();

            System.Array pr = new double[ds.Count+1];
            System.Array pi = new double[ds.Count+1];
            int i = 0;
            foreach (double d in ds)
            {
                pr.SetValue(d, i);
                i++;
                pi.SetValue(i, i);
            }
            
            
            matlab.PutFullMatrix("plottable", "base", pi, pr);
            matlab.Execute("plot(plottable)");
            
           
        }



        protected override void Update(GameTime gameTime)
        {
            
            Task.Factory.StartNew(() => logO());
            Task.WaitAll();
            if (level.lives >= 0)
            {  
                frameCounter++;

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();

                if (level.getScore() > 30 && lev <= 1)
                {
                    level = new Level(Content.Load<Map>("maps/" + mapNames[++lev]), Content, GraphicsDevice, shootEffect, mineEffect, shipDeadEffect, enemyDeadEffect);
                }

                input.Update();

                if (freeCam)
                {
                    if (input.IsKeyDown(Keys.W))
                        freeCamera.MoveForward();
                    if (input.IsKeyDown(Keys.S))
                        freeCamera.MoveBackward();
                    if (input.IsKeyDown(Keys.D))
                        freeCamera.MoveRight();
                    if (input.IsKeyDown(Keys.A))
                        freeCamera.MoveLeft();
                    if (input.IsKeyDown(Keys.P))
                        freeCamera.MoveUp();
                    if (input.IsKeyDown(Keys.L))
                        freeCamera.MoveDown();
                    if (input.IsKeyDown(Keys.O))
                        freeCamera.LookUp();
                    if (input.IsKeyDown(Keys.K))
                        freeCamera.LookDown();
                    if (input.IsKeyDown(Keys.I))
                        freeCamera.LookLeft();
                    if (input.IsKeyDown(Keys.J))
                        freeCamera.LookRight();

                    freeCamera.Update();
                }

                if (!freeCam && input.IsPressed(Keys.C))
                    currentCamera = 1 - currentCamera;

                if (input.IsKeyDown(Keys.Left))
                    level.ship.RotateShipLeft();
                if (input.IsKeyDown(Keys.Right))
                    level.ship.RotateShipRight();
                if (input.IsKeyDown(Keys.Up))
                    level.ship.Accerlerate();
                if (input.IsPressed(Keys.Y))
                    level.ship.MoveShipUp();
                if (input.IsPressed(Keys.G))
                    level.ship.MoveShipDown();
                if (input.IsPressed(Keys.Space))
                    level.ShipShoot();
                if (input.IsPressed(Keys.LeftControl))
                    level.ShipMine();
                Task.Factory.StartNew(() => level.Update(gameTime));
                
                
                if (input.IsPressed(Keys.Q))
                    freeCam = !freeCam;

                Matrix shipWorld = level.ship.World;
                Vector3 look = shipWorld.Forward;
                look.Normalize();

                camera1.Update(shipWorld.Translation + (1f * -look) + (0.5f * shipWorld.Up), shipWorld.Translation + shipWorld.Forward, shipWorld.Up);
                camera2.Update(shipWorld.Translation + new Vector3(5, 15, 5), shipWorld.Translation, shipWorld.Up);

                elapsedTime += gameTime.ElapsedGameTime;
                
                if (elapsedTime > TimeSpan.FromSeconds(1))
                {
                    //log.Run();
                    elapsedTime -= TimeSpan.FromSeconds(1);
                    frameRate = frameCounter;
                    frameCounter = 0;
                }

                if (drawPath)
                    path.Update(level.getEnemyPath());
                //log.run();
                Task.Factory.StartNew(() => base.Update(gameTime));
            }
        }

        private double avg(LinkedList<double> list)
        {
            double sum = 0, count = 0;
            LinkedList<double>.Enumerator a = list.GetEnumerator();
            while(a.MoveNext()){
                sum += a.Current;
                count++;
            }
            return sum/count;
        }

       
        protected void draw_rects()
        {
            spriteBatch.Begin();

            if (level.lives >= 0)
            {
                spriteBatch.DrawString(font, "Score: " + level.getScore() +
                    "\nShip Health: " + level.ship.getHealth() +
                    "\nEnemy Health: " + level.enemy.getHealth() +
                    "\nLives Left: " + level.lives +
                    "\nLevel: " + lev +
                    "\nFrame rate: " + frameRate, new Vector2(10, 10), Color.Red);
                Task.Factory.StartNew(() => DrawRectangle(new Rectangle(350, 280, 100, 20), up_color));
                Task.Factory.StartNew(() => DrawRectangle(new Rectangle(350, 450, 100, 20), down_color));
                Task.Factory.StartNew(() => DrawRectangle(new Rectangle(120, 325, 20, 80), left_color));
                Task.Factory.StartNew(() => DrawRectangle(new Rectangle(660, 325, 20, 80), right_color));
                up_color = Color.Black;
                down_color = Color.Black;
                left_color = Color.Black;
                right_color = Color.Black;
            }
            else
            {
                spriteBatch.DrawString(font, "GAME OVER", new Vector2(25, 25), Color.Black);
            }
            spriteBatch.End();
               
        }

        private void DrawRectangle(Rectangle coords, Color color)
        {
            var rect = new Texture2D(GraphicsDevice, 1, 1);
            rect.SetData(new[] { color });
            spriteBatch.Draw(rect, coords, color);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix view, proj;
            Vector3 up, right;

            if (!freeCam)
            {
                view = currentCamera == 0 ? camera1.View : camera2.View;
                proj = currentCamera == 0 ? camera1.Proj : camera2.Proj;
                up = currentCamera == 0 ? camera1.Up : camera2.Up;
                right = currentCamera == 0 ? camera1.Right : camera2.Right;
            }
            else
            {
                view = freeCamera.View;
                proj = freeCamera.Proj;
                up = freeCamera.Up;
                right = freeCamera.Right;
            }

            if (drawLevel)
                level.Draw(gameTime, textured, view, proj, up, right);

            if (drawGrid)
                grid.Draw(view, proj);
            if (drawXYZ)
                xyz.Draw(view, proj);
            if (drawPaths)
                paths.Draw(view, proj);
            if (drawPath)
                path.Draw(view, proj);


            spriteBatch.Begin();

            if (level.lives >= 0)
            {
                spriteBatch.DrawString(font, "Score: " + level.getScore() +
                    "\nShip Health: " + level.ship.getHealth() +
                    "\nEnemy Health: " + level.enemy.getHealth() +
                    "\nLives Left: " + level.lives +
                    "\nLevel: " + lev +
                    "\nFrame rate: " + frameRate, new Vector2(10, 10), Color.Red);
                DrawRectangle(new Rectangle(350, 280, 100, 20), up_color);
                DrawRectangle(new Rectangle(350, 450, 100, 20), down_color);
                DrawRectangle(new Rectangle(120, 325, 20, 80), left_color);
                DrawRectangle(new Rectangle(660, 325, 20, 80), right_color);
                up_color = Color.Black;
                down_color = Color.Black;
                left_color = Color.Black;
                right_color = Color.Black;        
            }
            else
            {
                spriteBatch.DrawString(font, "GAME OVER", new Vector2(25, 25), Color.Black);
            }
            spriteBatch.End();
            
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            base.Draw(gameTime);
        }
    }
}
