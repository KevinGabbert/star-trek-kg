using System;
using System.Collections.Generic;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Actors
{
    //TODO: Not Implemented Yet..
    public class Starbase : ISystem, IShip
    {
        public IGame Game { get; set; }

        public ISector Sector
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Destroyed
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Energy
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

        public IMap Map
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

        public FactionName Faction { get; set; }


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

        public Region GetRegion()
        {
            throw new NotImplementedException();
        }

        public Location GetLocation()
        {
            throw new NotImplementedException();
        }

        public void UpdateDivinedSectors()
        {
            throw new NotImplementedException();
        }

        public void Scavenge(ScavengeType scavengeType)
        {
            throw new NotImplementedException();
        }

        public void RepairEverything()
        {
            throw new NotImplementedException();
        }

        public string GetConditionAndSetIcon()
        {
            throw new NotImplementedException();
        }

        public bool AtLowEnergyLevel()
        {
            throw new NotImplementedException();
        }

        public string GetSubCommand()
        {
            throw new NotImplementedException();
        }

        public void ResetPrompt()
        {
            throw new NotImplementedException();
        }

        public void OutputLine(string textToOutput)
        {
            throw new NotImplementedException();
        }

        public void ClearOutputQueue()
        {
            throw new NotImplementedException();
        }

        public Queue<string> OutputQueue()
        {
            throw new NotImplementedException();
        }


        public Type Type()
        {
            throw new NotImplementedException();
        }

        List<string> IShipUI.OutputQueue()
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
