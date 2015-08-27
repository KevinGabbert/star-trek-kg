using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Output
{
    public class TelnetOutput : IOutputMethod
    {
        #region Properties

        /// <summary>
        /// Not used in this object
        /// </summary>
        public Queue<string> Queue { get; set; }

        public static TcpListener server { get; set; }
        public static TcpClient client { get; set; }
        public static NetworkStream stream { get; set; }
        public IStarTrekKGSettings Config { get; set; }

        public static bool TelnetServerInstantiated { get; set; }

        #endregion

        public TelnetOutput(IStarTrekKGSettings config)
        {
            this.Config = config;

            if (!TelnetOutput.TelnetServerInstantiated)
            {
                TelnetOutput.server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8081); //todo: resource both params
                TelnetOutput.server.Start();

                TelnetOutput.TelnetServerInstantiated = true;
                TelnetOutput.client = server.AcceptTcpClient();

                //execution stops while waiting for a connection.  This will need to be made into multiple threads.

                // Get a stream object for reading and writing
                TelnetOutput.stream = client.GetStream();
            }
        }
     
        #region IOutput Members

        public void Clear()
        {
            this.TWrite("'Clear()' Not Implemented Yet");
        }

        public string ReadLine()
        {
           return this.TReadLine();
        }

        public void Write(string text)
        {
            this.TWrite(text);
        }

        public List<string> Write(List<string> textLines)
        {
            throw new NotImplementedException();
        }

        public string WriteLine()
        {
            this.TWriteLine("");

            return null;
        }

        public List<string> WriteLine(string text)
        {
            this.TWriteLine(text);

            return null;
        }

        public void WriteLine(string text, object text2)
        {
            this.TWriteLine(text, text2.ToString());
        }

        public void WriteLine(string text, object text2, object text3)
        {
            this.TWriteLine(text, text2.ToString(), text3.ToString());
        }

        public void WriteLine(string text, object text2, object text3, object text4)
        {
            this.TWriteLine(text, text2.ToString(), text3.ToString(), text4.ToString());
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

        #endregion

        private string TReadLine()
        {
            string data = null;

            byte[] bytes = new byte[256];

            // Incoming message may be larger than the buffer size. 
            do
            {
                var numBytes = TelnetOutput.stream.Read(bytes, 0, bytes.Length);
                data += Encoding.ASCII.GetString(bytes, 0, numBytes);

            } while (TelnetOutput.stream.DataAvailable);

            return data;
        }
        private void TWrite(params string[] messages)
        {
            foreach (byte[] messageOut in messages.Select(message => Encoding.UTF8.GetBytes(message)))
            {
                TelnetOutput.stream.Write(messageOut, 0, messageOut.Length);
            }
        }

        private void TWriteLine(params string[] messages)
        {
            foreach (byte[] messageOut in messages.Select(message => Encoding.UTF8.GetBytes(message + "\r\n")))
            {
                TelnetOutput.stream.Write(messageOut, 0, messageOut.Length);
            }
        }
    }
}
