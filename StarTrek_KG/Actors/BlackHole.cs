using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Actors
{
    public class BlackHole: IStar
    {
        //Not Implemented yet..
        //on construct, set my gravity to be reeel high
        //(basically), this affects how many sectors around the black hole can pull the ship in
        public ICoordinate Coordinate
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

        Type ICoordinateObject.Type
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

        string ICoordinateObject.Name { get; set; }
    }
}