using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Actors
{
    public class Star : IStar
    {
        public Star()
        {
            this.Type = this.GetType();
        }

        public ISector Sector
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Type Type { get; set; }

        public int Mass { get; set; }

        public string Name { get; set; } //todo: pulled from a list of star names

        public string Designation { get; set; } //So we can find dupes

        public int Gravity { get; set; }
    }
}
