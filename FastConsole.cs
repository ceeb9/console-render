using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FastConsole
{
    class FastWrite
    {
        //i have no idea what the shit below does
        //setup for windows api i think
        //copy pasted from stack overflow so DO NOT TOUCH
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutputW(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        private struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        private struct CharUnion
        {
            [FieldOffset(0)] public ushort UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }


        //real shit starts here
        [STAThread]
        public static void Write(int width, int height, char[] pixels, short[] colors)
        {
            SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (!h.IsInvalid)
            {
                CharInfo[] buf = new CharInfo[width * height];
                SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)(width), Bottom = (short)height };

                for (int i = 0; i < pixels.Length; i++)
                {
                    /*
                    0 black
                    1 blue
                    2 green
                    3 cyan      
                    4 red
                    5 purple
                    6 yellow
                    7 white
                    */

                    /*first 3 bit number indicates foreground colour
                    second 3 bit number indicates foreground brightness (8 or 0)
                    third 3 bit number indicates background colour
                    fourth 3 bit number indicates background brightness (8 or 0)*/
                    /*
                    RECIPE FOR MAKING COLORS
                    COLOR:
                        B: +1
                        G: +2
                        R: +4
                        BRIGHT: +8

                    BACKGROUND: 
                        ON: +16
                        BRIGHT: +32
                    */

                    buf[i].Attributes = (short)(colors[i]);
                    buf[i].Char.UnicodeChar = pixels[i];
                }



                bool b = WriteConsoleOutputW(h, buf,
                  new Coord() { X = (short)width, Y = (short)height },
                  new Coord() { X = 0, Y = 0 },
                  ref rect);
            }
        }
    }
}