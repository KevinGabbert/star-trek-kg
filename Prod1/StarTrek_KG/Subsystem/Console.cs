using System;
using C = System.Console;

namespace StarTrek_KG.Subsystem
{
    //todo: mock this out for tests.
    public class Console
    {
        public void Write(string text)
        {
            C.Write(text);
        }

        public void WriteLine()
        {
            C.WriteLine("");
        }

        public void WriteLine(string text)
        {
            C.WriteLine(text);
        }

        public void WriteLine(string text, object text2)
        {
            C.WriteLine(text, Convert.ToString(text2));
        }

        public void WriteLine(string text, object text2, object text3)
        {
            C.WriteLine(text, Convert.ToString(text2), Convert.ToString(text3));
        }

        public void WriteLine(string text, object text2, object text3, object text4)
        {
            C.WriteLine(text, Convert.ToString(text2), Convert.ToString(text3), Convert.ToString(text4));
        }

        public string ReadLine()
        {
            return C.ReadLine();
        }
    }
}
