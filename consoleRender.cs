using System;
using System.Drawing;
using System.Collections.Generic;
using ConsoleImage;
using System.Threading;
using System.Diagnostics;

//to do
//refactor final writing pipeline around new windows api function - DONE
//add support for colour with the attributes value of the new drawing method - DONE
//optimize for performance - TO DO
//add input support - DONE

namespace ConsoleRender
{
    public enum ShapeType
    {
        Point,
        Line,
        Rectangle,
        Circle,
        Text
    };
    public class ShapeObject
    {
        public PointF pt1; //top left point on bounding box
        public PointF pt2; //width + height of bounding box
                           //NOT COORDINATES!!!!!!!!!!! WIDTH!!!!!!!!!!
        public PointF centre;
        public string name;
        public int type;
        public float rotation;
        public string text;
        public Font font;
        public Color color;
        public bool fill;
    }
    public class Canvas
    {
        //canvas properties
        public int Width;
        public int Height;
        public string currentMessage = ""; //for debug console
        //------------------

        //object list and required methods
        public Bitmap bmpOutput;
        public List<ShapeObject> objectList = new List<ShapeObject>(); //list of objects on the canvas
        public Dictionary<string, int> objectDict = new Dictionary<string, int>(); //store the name of the object and its index
        public ShapeObject lookup(string item) //simple exposed method to get the index of an object's name
        {
            return objectList[objectDict[item]];
        }
        //--------------------------------

        //statics and helper methods
        private static Font defaultFont = new Font("Arial", 1f);

        //update related
        public TickHandler inputHandler;
        public RenderHandler renderHandler;
        public ImageToText textImage = new ImageToText();
        //-------------

        //methods
        //setup method
        public Canvas(int widthIn, int heightIn, Action<RenderHandler.FrameInfo> renderMethod, Action<TickHandler.TickInfo> onTickMethod)
        {
            //initialize console properties
            this.Width = widthIn;
            this.Height = heightIn;
            bmpOutput = new Bitmap(Width+1, Height+1); //dont ask me why just fucking deal with it

            //initialize rendering loop and input thread
            renderHandler = new RenderHandler(this, renderMethod);
            inputHandler = new TickHandler(onTickMethod, this);
        }

        //method for constructing a shape object from given parameters
        public void CreateShape(string name, int type, int x, int y, int width = 0, int height = 0, bool fill = false, Color? color = null, float rotation = 0, string text = "")
        {
            //create a new shape object that fits the parameters given
            ShapeObject shape = new ShapeObject();
            shape.name = name;
            shape.pt1.X = x;
            shape.pt1.Y = y;
            shape.pt2.X = width;
            shape.pt2.Y = height;
            shape.type = type;
            shape.rotation = rotation;
            shape.text = text;
            shape.font = defaultFont;
            shape.fill = fill;
            shape.color = color ?? Color.White; //set it to the specified color or white if null

            shape.centre.X = shape.pt1.X + shape.pt2.X / 2;
            shape.centre.Y = shape.pt1.Y + shape.pt2.Y / 2;

            objectList.Add(shape); //add it to the object list for the canvas
            objectDict.Add(shape.name, objectList.Count - 1); //add the object to the lookup table
        }

        //the big one -- method for taking shapes from the canvas' list and drawing them onto a bitmap
        public void Render(bool dither)
        {
            //DRAW POLYGONS ONTO IMAGE
            using (Graphics gr = Graphics.FromImage(bmpOutput))
            {
                for (int i = 0; i < objectList.Count; i++)
                {
                    //set up the canvas for rotation
                    gr.TranslateTransform(objectList[i].pt1.X + objectList[i].pt2.X / 2, objectList[i].pt1.Y + objectList[i].pt2.Y / 2);
                    gr.RotateTransform(objectList[i].rotation);
                    gr.TranslateTransform(-1 * (objectList[i].pt1.X + objectList[i].pt2.X / 2), -1 * (objectList[i].pt1.Y + objectList[i].pt2.Y / 2));

                    //parse shape objects into graphics library draw method commands
                    Pen color = new Pen(new SolidBrush(objectList[i].color));
                    color.Width = 1f;
                    Brush brush = new SolidBrush(objectList[i].color);
    
                    switch (objectList[i].type)
                    {
                        case (int)ShapeType.Point:
                            {
                                bmpOutput.SetPixel((int)objectList[i].pt1.X, (int)objectList[i].pt1.Y, objectList[i].color);
                            }
                            break;

                        case (int)ShapeType.Line:
                            {

                                gr.DrawLine(color, objectList[i].pt1, objectList[i].pt2);
                            }
                            break;

                        case (int)ShapeType.Rectangle:
                            {
                                
                                if (objectList[i].fill){
                                    gr.FillRectangle(brush, new RectangleF(objectList[i].pt1.X, objectList[i].pt1.Y, objectList[i].pt2.X, objectList[i].pt2.Y));
                                }
                                else{
                                    gr.DrawRectangle(color, objectList[i].pt1.X, objectList[i].pt1.Y, objectList[i].pt2.X, objectList[i].pt2.Y);
                                }
                            }
                            break;

                        case (int)ShapeType.Circle:
                            {
                                
                                if (objectList[i].fill){
                                    gr.FillEllipse(brush, new RectangleF(objectList[i].pt1, new SizeF(objectList[i].pt2)));
                                }
                                else{
                                    gr.DrawEllipse(color, objectList[i].pt1.X, objectList[i].pt1.Y, objectList[i].pt2.X, objectList[i].pt2.Y);
                                }
                            }
                            break;

                        case (int)ShapeType.Text:
                            {
                                //Font tempFont = GetAdjustedFont(gr, objectList[i].text, defaultFont, (int)objectList[i].pt2.X);
                                gr.DrawString(objectList[i].text, objectList[i].font, brush, objectList[i].pt1);
                            }
                            break;

                        default:
                            break;
                    }

                    //reset the canvas for next shape's rotation
                    gr.ResetTransform();
                    brush.Dispose();
                    color.Dispose();
                }
            }
            //send to text rendering
            textImage.applyColor(bmpOutput);
        }
    }
    public class TickHandler
    {
        public static int currentLine = 1; //keep track of input line 
        //initialize a new thread that handles input and modifications to objects in the scene
        public TickHandler(Action<TickHandler.TickInfo> onTickMethod, Canvas cv)
        {
            Thread tl = tickLoopStart(onTickMethod, cv);
        }
        private Thread tickLoopStart(Action<TickHandler.TickInfo> onTickMethod, Canvas cv)
        {
            var t = new Thread(() => TickLoop(onTickMethod, cv));
            t.Start();
            return t;
        }
        //------------------------------------------------------------------------------------- (i know it's jank but that's how threads are)

        //method containing the actual loop for ticks
        private static void TickLoop(Action<TickHandler.TickInfo> onTickMethod, Canvas cv)
        {
            
            Console.CursorVisible = false;
            while (true)
            {
                //get information for user to use
                TickInfo ti = new TickInfo();

                //methods for populating tick info go here
                ti.key = TickInfoMethods.updateKey();
                //----------------------------------------

                //methods for debug console
                if (ti.key == ConsoleKey.OemPeriod || cv.currentMessage != "")
                {
                    TickInfoMethods.debugConsole(cv, currentLine);
                }
                
                //start the user written code for this tick
                onTickMethod(ti);
            }
        }

        //holds methods for getting current tick info
        private static class TickInfoMethods
        {
            public static ConsoleKey updateKey() //method to get the current key input
            {
                if (Console.KeyAvailable)
                {
                    return Console.ReadKey().Key;
                }
                else
                {
                    return new ConsoleKey();
                }
            }

            public static void debugConsole(Canvas cv, int line)
            {
                //write header
                Console.CursorVisible = false;
                Console.SetCursorPosition(cv.textImage.Width + 4, 0);
                Console.Write("Objects: {0}", cv.objectList.Count);

                Console.SetCursorPosition(cv.textImage.Width + 3, line);
                Console.Write('>');
                Console.CursorVisible = true;
                string input = "";//Console.ReadLine();

                if(cv.currentMessage != "")
                {
                    Console.SetCursorPosition(cv.textImage.Width + 3, line+1);
                    Console.Write(">" + cv.currentMessage);
                    cv.currentMessage = "";
                }

                //do command parsing n shit here
                if(input=="width")
                {
                    Console.SetCursorPosition(cv.textImage.Width + 3, line+1);
                    Console.Write(">" + cv.textImage.Width);
                    
                }
                if(input=="height")
                {
                    Console.SetCursorPosition(cv.textImage.Width + 3, line+1);
                    Console.Write(">" + cv.textImage.Height);
                }

                Console.CursorVisible = false;
                currentLine+=2;
            }
        }
        public class TickInfo //a class which holds info about input and other things from the current tick
        {
            public ConsoleKey key;
        }
    }
    public class RenderHandler //contains objects that handle the rendering loop
    {
        private Action<RenderHandler.FrameInfo> localRenderMethod;
        private Canvas localcv;
        private int lastConsoleWidth = 0;
        private int lastConsoleHeight = 0;
        public bool dither = true;
        public RenderHandler(Canvas cv, Action<RenderHandler.FrameInfo> renderMethod)
        {
            localRenderMethod = renderMethod;
            localcv = cv;
        }
        public void startRendering() //main render loop
        {
            FrameInfo fi = new FrameInfo();
            Stopwatch s = new Stopwatch();
            Console.SetCursorPosition(localcv.textImage.Width + 4, 1);

            s.Start();
            Thread.Sleep(100);
            while (true)
            {
                fi.frametime = s.ElapsedMilliseconds;
                s.Restart();
                checkForResize();

                localRenderMethod(fi); //run user code

                localcv.Render(dither);
            }
        }
        private void checkForResize() //check if the window has been resized 
                                      //clear it if it has to avoid artifacting
        {
            if (lastConsoleWidth != Console.WindowWidth)
            {
                Console.Clear();
                lastConsoleWidth = Console.WindowWidth;
            }
            if (lastConsoleHeight != Console.WindowHeight)
            {
                Console.Clear();
                lastConsoleHeight = Console.WindowHeight;
            }
        }
        public class FrameInfo //a class which holds info about a frame
        {
            public long frametime;
        }
    }

}
