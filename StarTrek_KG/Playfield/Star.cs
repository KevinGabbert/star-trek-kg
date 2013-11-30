using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class Star : IStar
    {
        public Sector Sector
        {
            get { throw new global::System.NotImplementedException(); }
        }

        public Type Type
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
