using System;
using ConsoleRender;
using System.Drawing;
using ColorManipulation;
using System.Diagnostics;
using Boids;

namespace Program
{
    class Program
    {
        public static Canvas c = new Canvas(128, 128, new Action<RenderHandler.FrameInfo>(OnFrame), new Action<TickHandler.TickInfo>(OnTick));
        public static float[] frametimes = new float[5];
        public static BoidLogic b = new BoidLogic(200, c);
        static void Main(string[] args)
        {
            Debugger.Launch();
            OnStart();
            c.renderHandler.startRendering();
            Console.Clear();
        }
        //user code that runs once before rendering starts
        public static void OnStart()
        {
            c.CreateShape("bg", (int)ShapeType.Rectangle, 0, 0, c.Width, c.Height, true, Color.White);
            c.CreateShape("border", (int)ShapeType.Rectangle, 0, 0, c.Width, c.Height, false, Color.Purple);
            for (int i = 0; i < b.boids.Count; i++)
            {
                b.boids[i].pos.X = new Random().Next(0, c.Width);
                b.boids[i].pos.Y = new Random().Next(0, c.Height);
                c.CreateShape(i.ToString(), (int)ShapeType.Rectangle, 32, 32, 2, 2, fill: true, color: Color.Black, 0);
                //c.CreateShape(i.ToString()+"c", (int)ShapeType.Circle, 32, 32, b.boids[i].detectRange*2, b.boids[i].detectRange*2, color:Color.FromArgb(85, Color.Black));
            }

        }
        //user code that runs each time the frame is rendered
        public static void OnFrame(RenderHandler.FrameInfo fi)
        {
            //code for averaging frametimes (retarded)
            frametimes[frametimes.Length - 1] = 0;
            float[] tempFrametimeHolder = new float[frametimes.Length];
            for (var i = 0; i < frametimes.Length - 1; i++)
            {
                tempFrametimeHolder[i + 1] = frametimes[i];
            }
            frametimes = tempFrametimeHolder;
            frametimes[0] = fi.frametime;
            float avg = 0;
            for (var i = 0; i < frametimes.Length; i++)
            {
                avg += frametimes[i];
            }
            avg = avg / frametimes.Length;
            //c.lookup("framerate").text = Math.Round(((float)(1000f / (float)avg))).ToString();
            //------------------------------------------

            b.boidTick();
            for (int i = 0; i < b.boids.Count; i++)
            {
                //set color for debugging
                if (b.boids[i].nearbyBoids.Count > 1)
                {
                    c.lookup(i.ToString()).color = Color.Blue;
                }
                else
                {
                    c.lookup(i.ToString()).color = Color.Black;
                }

                //prevent going off screen
                //float angToCentre = (float)Math.Atan2(((c.Height / 2) - b.boids[i].y), ((c.Width / 2) - b.boids[i].x));
                /*if (b.boids[i].pos.X > c.Width-2)
                {
                    b.boids[i].pos.X = 3;
                }
                if (b.boids[i].pos.X < 2)
                {
                    b.boids[i].pos.X = c.Width-3;
                }
                if (b.boids[i].pos.Y > c.Height-2)
                {
                    b.boids[i].pos.Y = 3;
                }
                if (b.boids[i].pos.Y < 2)
                {
                    b.boids[i].pos.Y = c.Height-3;
                }*/

                //update pos
                c.lookup(i.ToString()).pt1.X = b.boids[i].pos.X;
                c.lookup(i.ToString()).pt1.Y = b.boids[i].pos.Y;

                //update circle pos
                //c.lookup(i.ToString()+"c").pt1.X = b.boids[i].pos.X-b.boids[i].detectRange;
                //c.lookup(i.ToString()+"c").pt1.Y = b.boids[i].pos.Y-b.boids[i].detectRange;
            }



        }
        //user code that runs on a separate thread to the renderer
        public static void OnTick(TickHandler.TickInfo Tick) //put things that the user can change or interact with directly in here
        {                                                    //NEVER EVER put anything outside of input conditionals here - will cause the program to crash
            if (Tick.key == ConsoleKey.Escape)
            {
                Console.SetCursorPosition(0, c.textImage.Height / 2);
                System.Environment.Exit(0);
            }
        }
    }

}