using System;

namespace StarTrek_KG.Exceptions
{
    public class GameConfigException : Exception
    {
        public GameConfigException(string message): base(message)
        {
        }
    }
}
