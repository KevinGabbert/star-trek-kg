using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Actors
{
    public class BlackHole: IStar
    {
        //Not Implemented yet..
        //on construct, set my gravity to be reeel high
        //(basically), this affects how many sectors around the black hole can pull the ship in
        public ISector Sector
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Type Type()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public int Gravity
        {
            get { throw new NotImplementedException(); }
        }

        Type ISectorObject.Type
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

        string ISectorObject.Name { get; set; }
    }
}