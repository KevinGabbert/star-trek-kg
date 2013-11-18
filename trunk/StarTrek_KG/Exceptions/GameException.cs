using System;

namespace StarTrek_KG.Exceptions
{
    public class GameException : Exception
    {
        public GameException(string message): base(message)
        {
        }
    }
}
