﻿using System;
using System.Collections.Generic;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Output
{
    public class SubscriberWrite : IOutput
    {
        private IStarTrekKGSettings config;
        private List<string> ACTIVITY_PANEL;

        public Queue<string> OutputQueue { get; set; }

        public SubscriberWrite(IStarTrekKGSettings config)
        {
            this.ACTIVITY_PANEL = new List<string>();
            this.Config = config;

            this.OutputQueue = new Queue<string>();
        }

        public IStarTrekKGSettings Config { get; set; }

        public void Clear()
        {
           
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
            //System.Console.WriteLine(text, Convert.ToString(text2));
            this.Enqueue(text);
        }

        public void WriteLine(string text, object text2, object text3)
        {
            //System.Console.WriteLine(text, Convert.ToString(text2), Convert.ToString(text3));
            this.Enqueue(text);
        }

        public void WriteLine(string text, object text2, object text3, object text4)
        {
            // System.Console.WriteLine(text, Convert.ToString(text2), Convert.ToString(text3), Convert.ToString(text4));
            this.Enqueue(text);
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

        //public void SetWindowSize()
        //{
        //    const string m1 = "The current window width is {0}, and the current window height is {1}.";
        //    const string m2 = "The new window width is {0}, and the new window height is {1}.";
        //    const string m4 = "  (Press any key to continue...)";

        //    // 
        //    // Step 1: Get the current window dimensions. 
        //    //
        //    int origWidth = System.Console.WindowWidth;
        //    int origHeight = System.Console.WindowHeight;
        //    System.Console.WriteLine(m1, System.Console.WindowWidth, System.Console.WindowHeight);
        //    System.Console.WriteLine(m4);
        //    System.Console.ReadKey(true);

        //    // 
        //    // Step 2: Cut the window to 1/4 its original size. 
        //    //
        //    int width = origWidth / 2;
        //    int height = origHeight / 2;
        //    System.Console.SetWindowSize(width, height);
        //    System.Console.WriteLine(m2, System.Console.WindowWidth, System.Console.WindowHeight);
        //    System.Console.WriteLine(m4);
        //    System.Console.ReadKey(true);

        //    // 
        //    // Step 3: Restore the window to its original size. 
        //    //
        //    System.Console.SetWindowSize(origWidth, origHeight);
        //    System.Console.WriteLine(m1, System.Console.WindowWidth, System.Console.WindowHeight);
        //}
    }
}