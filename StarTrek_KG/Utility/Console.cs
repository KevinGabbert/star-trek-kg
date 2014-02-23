using System;

namespace StarTrek_KG.Utility
{
    //todo: mock this out for tests.
    public class Console
    {
        public void Write(string text)
        {
            System.Console.Write(text);
        }

        public void WriteLine()
        {
            System.Console.WriteLine("");
        }

        public void WriteLine(string text)
        {
            System.Console.WriteLine(text);
        }

        public void WriteLine(string text, object text2)
        {
            System.Console.WriteLine(text, Convert.ToString(text2));
        }

        public void WriteLine(string text, object text2, object text3)
        {
            System.Console.WriteLine(text, Convert.ToString(text2), Convert.ToString(text3));
        }

        public void WriteLine(string text, object text2, object text3, object text4)
        {
            System.Console.WriteLine(text, Convert.ToString(text2), Convert.ToString(text3), Convert.ToString(text4));
        }

        public string ReadLine()
        {
            return System.Console.ReadLine();
        }

        public void HighlightTextBW(bool on)
        {
            if (on)
            {
                System.Console.ForegroundColor = ConsoleColor.Black;
                System.Console.BackgroundColor = ConsoleColor.White;
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        public void SetWindowSize()
        {
            int origWidth, width;
            int origHeight, height;
            string m1 = "The current window width is {0}, and the " +
                        "current window height is {1}.";
            string m2 = "The new window width is {0}, and the new " +
                        "window height is {1}.";
            string m4 = "  (Press any key to continue...)";
            // 
            // Step 1: Get the current window dimensions. 
            //
            origWidth = System.Console.WindowWidth;
            origHeight = System.Console.WindowHeight;
            System.Console.WriteLine(m1, System.Console.WindowWidth,
                                  System.Console.WindowHeight);
            System.Console.WriteLine(m4);
            System.Console.ReadKey(true);
            // 
            // Step 2: Cut the window to 1/4 its original size. 
            //
            width = origWidth / 2;
            height = origHeight / 2;
            System.Console.SetWindowSize(width, height);
            System.Console.WriteLine(m2, System.Console.WindowWidth,
                                  System.Console.WindowHeight);
            System.Console.WriteLine(m4);
            System.Console.ReadKey(true);
            // 
            // Step 3: Restore the window to its original size. 
            //
            System.Console.SetWindowSize(origWidth, origHeight);
            System.Console.WriteLine(m1, System.Console.WindowWidth,
                                  System.Console.WindowHeight);
        }
    }
}
