using System;
using System.Collections.Generic;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Output
{
    public class SubscriberOutput : IOutputMethod
    {
        public IStarTrekKGSettings Config { get; set; }
        public Queue<string> Queue { get; set; }

        public SubscriberOutput(IStarTrekKGSettings config)
        {
            this.Config = config;
            this.Clear();
        }

        public void Clear()
        {
            this.Queue = new Queue<string>();
        }

        public void Enqueue(string text)
        {
            this.Queue.Enqueue(text.Replace("\r\n", ""));
        }

        public void Write(string text)
        {
            this.Enqueue(text);
        }

        public List<string> Write(List<string> textLines)
        {
            if (textLines == null)
            {
                return new List<string>();
            }

            foreach (var line in textLines)
            {
                this.Enqueue(line);
            }

            return textLines;
        }

        public string WriteLine()
        {
            string emptyLine = "";
            this.Enqueue(emptyLine);

            return emptyLine;
        }

        public List<string> WriteLine(string text)
        {
            List<string> lineToEnqueue = new List<string> {text + Environment.NewLine};

            foreach (var line in lineToEnqueue)
            {
                this.Enqueue(line);
            }

            return lineToEnqueue;
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

        public void HighlightTextBW(bool on)
        {
            //todo: implement this. (using ANSI codes?)
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

        /// <summary>
        /// Not used by this object
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            throw new NotImplementedException();
        }
    }
}
