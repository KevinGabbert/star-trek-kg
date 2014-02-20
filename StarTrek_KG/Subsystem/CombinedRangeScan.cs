using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    class CombinedRangeScan : SubSystem_Base
    {
        public CombinedRangeScan(Ship shipConnectedTo, Game game)
        {
            this.Game = game;
            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
            this.Type = SubsystemType.CombinedRangeScan;
        }

        public override void Controls(string command)
        {
            throw new NotImplementedException();
        }

        public void Controls()
        {
            //if (Damaged()) return;

            //var location = this.ShipConnectedTo.GetLocation();
            //Quadrant quadrant = Quadrants.Get(this.Game.Map, location.Quadrant);

            //var printSector = (new PrintSector(this.Game.Config.GetSetting<int>("ShieldsDownLevel"), (this.Game.Config.GetSetting<int>("LowEnergyLevel")),this.Game.Write, this.Game.Config));
            //printSector.SRSPrintSector(quadrant, this.Game.Map); 

            //quadrant.ClearSectorsWithItem(SectorItem.Debug); //Clears any debug Markers that might have been set

            //quadrant.Scanned = true;
        }

        public static CombinedRangeScan For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (CombinedRangeScan)."); //todo: reflect the name and refactor this to ISubsystem
            }

            return (CombinedRangeScan) ship.Subsystems.Single(s => s.Type == SubsystemType.ShortRangeScan);
        }
    }
}
