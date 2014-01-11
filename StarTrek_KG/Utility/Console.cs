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
    }
}
