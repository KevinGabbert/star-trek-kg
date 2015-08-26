using System;
using System.Collections.Generic;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Output
{
    public class SubscriberWrite : IOutput
    {
        public IStarTrekKGSettings Config { get; set; }

        public Queue<string> OutputQueue { get; set; }

        public SubscriberWrite(IStarTrekKGSettings config)
        {
            this.Config = config;
            this.Clear();
        }

        public void Clear()
        {
            this.OutputQueue = new Queue<string>();
        }

        public void Enqueue(string text)
        {
            this.OutputQueue.Enqueue($"<pre>{text.Replace("\r\n", "")}</pre>");
        }

        public void Write(string text)
        {
            this.Enqueue(text);
        }

        public void WriteLine()
        {
            this.Enqueue("");
        }

        public void WriteLine(string text)
        {
            this.Enqueue(text + Environment.NewLine);
        }

        public void WriteLine(string text, object text2)
        {
            this.Enqueue(string.Format(text, text2));
        }

        public void WriteLine(string text, object text2, object text3)
        {
            this.Enqueue(string.Format(text, text2, text3));
        }

        public void WriteLine(string text, object text2, object text3, object text4)
        {
            this.Enqueue(string.Format(text, text2, text3, text4));
        }

        public string ReadLine()
        {
            //return System.Console.ReadLine();
            //throw new NotImplementedException();

            return "";
        }

        public void HighlightTextBW(bool on)
        {
            if (on)
            {
                //System.Console.ForegroundColor = ConsoleColor.Black;
                //System.Console.BackgroundColor = ConsoleColor.White;
            }
            else
            {
                //System.Console.ForegroundColor = ConsoleColor.White;
                //System.Console.BackgroundColor = ConsoleColor.Black;
            }
        }
    }
}
