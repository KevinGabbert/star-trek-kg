using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
{
    public class BlackHole: IStar
    {
        //Not Implemented yet..
        //on construct, set my gravity to be reeel high
        //(basically), this affects how many sectors around the black hole can pull the ship in
        public Sector Sector
        {
            get { throw new global::System.NotImplementedException(); }
        }


        public global::System.Type Type()
        {
            throw new global::System.NotImplementedException();
        }

        public string Name
        {
            get { throw new global::System.NotImplementedException(); }
        }

        public int Gravity
        {
            get { throw new global::System.NotImplementedException(); }
        }


        global::System.Type ISectorObject.Type
        {
            get
            {
                throw new global::System.NotImplementedException();
            }
            set
            {
                throw new global::System.NotImplementedException();
            }
        }
    }
}