using System;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
{
    //TODO: Not Implemented Yet..
    public class Starbase : ISystem, IShip
    {
        public ISector Sector
        {
            get { throw new NotImplementedException(); }
        }

        public bool Destroyed
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public double Energy
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

        public Map Map
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

        public Enums.Allegiance Allegiance
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

        public string Name
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


        public Subsystem.Subsystems Subsystems
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


        public Coordinate Coordinate
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

        public Quadrant GetQuadrant()
        {
            throw new NotImplementedException();
        }


        public Type Type()
        {
            throw new NotImplementedException();
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
    }
}
