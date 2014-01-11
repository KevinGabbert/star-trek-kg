using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class ShortRangeScan : SubSystem_Base, IMap, ICommand, IWrite
    {
        public ShortRangeScan(Map map, Ship shipConnectedTo, Write write, Command command)
        {
            this.Write = write;
            this.Command = command;

            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
            this.Type = SubsystemType.ShortRangeScan;
        }

        public override void OutputDamagedMessage()
        {
            this.Write.Resource("SRSDamaged");
            this.Write.Resource("RepairsUnderway");
            this.Write.Line("Hint: You can use some computer functions to navigate without SRS"); //todo: can we make hints dismissable?
        }
        public override void OutputRepairedMessage()
        {
            this.Write.Line("Short range scanner has been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public override void Controls(string command)
        {
            throw new NotImplementedException();
        }

        public void Controls()
        {
            if (Damaged()) return;

            var location = this.ShipConnectedTo.GetLocation();
            Quadrant quadrant = Quadrants.Get(this.Map, location.Quadrant);

            var printSector = (new PrintSector(StarTrekKGSettings.GetSetting<int>("ShieldsDownLevel"), StarTrekKGSettings.GetSetting<int>("LowEnergyLevel"),this.Write, this.Command));
            printSector.Print(quadrant, this.Map); 

            quadrant.ClearSectorsWithItem(SectorItem.Debug); //Clears any debug Markers that might have been set

            quadrant.Scanned = true;
        }

        public new static ShortRangeScan For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (ShortRangeScan)."); //todo: reflect the name and refactor this to ISubsystem
            }

            return (ShortRangeScan) ship.Subsystems.Single(s => s.Type == SubsystemType.ShortRangeScan);
        }
    }
}
