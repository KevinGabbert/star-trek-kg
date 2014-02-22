using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace StarTrek_KG.Utility
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ConsoleFont
    {
        public readonly uint Index;
        public readonly short SizeX;
        public readonly short SizeY;
    }

    public static class ConsoleHelper
    {
        [DllImport("kernel32")]
        public static extern bool SetConsoleIcon(IntPtr hIcon);

        public static bool SetConsoleIcon(Icon icon)
        {
            return SetConsoleIcon(icon.Handle);
        }

        [DllImport("kernel32")]
        private extern static bool SetConsoleFont(IntPtr hOutput, uint index);

        private enum StdHandle
        {
            OutputHandle = -11
        }

        [DllImport("kernel32")]
        private static extern IntPtr GetStdHandle(StdHandle index);

        public static bool SetConsoleFont(uint index)
        {
            return SetConsoleFont(GetStdHandle(StdHandle.OutputHandle), index);
        }

        [DllImport("kernel32")]
        private static extern bool GetConsoleFontInfo(IntPtr hOutput, [MarshalAs(UnmanagedType.Bool)]bool bMaximize,
            uint count, [MarshalAs(UnmanagedType.LPArray), Out] ConsoleFont[] fonts);

        [DllImport("kernel32")]
        private static extern uint GetNumberOfConsoleFonts();

        public static uint ConsoleFontsCount
        {
            get
            {
                return GetNumberOfConsoleFonts();
            }
        }

        public static ConsoleFont[] ConsoleFonts
        {
            get
            {
                var fonts = new ConsoleFont[GetNumberOfConsoleFonts()];
                if (fonts.Length > 0)
                    GetConsoleFontInfo(GetStdHandle(StdHandle.OutputHandle), false, (uint)fonts.Length, fonts);
                return fonts;
            }
        }

    }

    //sample usage:

   //    var fonts = ConsoleHelper.ConsoleFonts;
   //for(int f = 0; f < fonts.Length; f++)
   //   Console.WriteLine("{0}: X={1}, Y={2}",
   //      fonts[f].Index, fonts[f].SizeX, fonts[f].SizeY);


   //ConsoleHelper.SetConsoleFont(5);

    //ConsoleHelper.SetConsoleIcon(SystemIcons.Information);
}
