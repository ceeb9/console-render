using System;
using System.Drawing;
using ColorManipulation;

namespace ConsoleImage
{
    public class ImageToText
    {
        //image variables
        private Bitmap imageScaledDithered;
        private Color[,] scaleDitherColor;
        public int Width;
        public int Height;

        //populate char array with upper blocks
        //fills color
        //then writes it to the screen
        public void applyColor(Bitmap bmp)
        {
            windowResize(bmp);
            imageScaledDithered = resizeBitmap(Conversions.ColorToBmp(Calculations.ditherValues(bmp)), Width, Height);
            
            scaleDitherColor = Conversions.ColorFromBmp(imageScaledDithered);

            char[] filledGrid = new char[scaleDitherColor.GetLength(0) * scaleDitherColor.GetLength(1) / 2];
            Array.Fill<char>(filledGrid, '▀');

            short[] colorShort = new short[scaleDitherColor.Length / 2];
            //the final output array of charinfo color attributes
            //set colorshort to the values of the row + the row below it
            //row 0 : foreground colors
            //row 1 :background colors
            //these need to be merged in colorshort
            int i = 0;
            for (int y = 0; y < scaleDitherColor.GetLength(1); y += 2)
            {
                for (int x = 0; x < scaleDitherColor.GetLength(0); x++)
                {
                    colorShort[i] = (short)(Conversions.ColorToShort(scaleDitherColor[x, y], true) + Conversions.ColorToShort(scaleDitherColor[x, y + 1], false));
                    i++;
                }
            }
            FastConsole.FastWrite.Write(scaleDitherColor.GetLength(0), scaleDitherColor.GetLength(1) / 2, filledGrid, colorShort);
        }
        private Bitmap resizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor; 
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }
        public void windowResize(Bitmap bmp) //main method that calls all other functions, this is the function called by the user
        {

            Width = Console.WindowHeight*2; 
            Height = (Console.WindowHeight*2); 

            //adjust the buffers accordingly
            if (Width >= Console.BufferWidth)
            {
                Console.BufferWidth = Width + 1; //plus 1 to account for newline characters
            }
            if (Height >= Console.BufferHeight)
            {
                Console.BufferHeight = Height + 1;
            }
        }
    }
}

