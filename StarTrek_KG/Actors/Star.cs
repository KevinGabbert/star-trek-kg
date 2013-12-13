using System;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
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

        public int Mass { get; set; }
    }
}
