using System;
using System.Linq;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class ShortRangeScan : SubSystem_Base, IMap
    {
        public ShortRangeScan(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.ShortRangeScan;
        }

        public override void OutputDamagedMessage()
        {
            Output.ShortRangeScanDamageMessage();
        }
        public override void OutputRepairedMessage()
        {
            Output.WriteLine("Short range scanner has been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public override void Controls(string command)
        {
            throw new NotImplementedException();
        }

        public void Controls(Map map)
        {
            if (Damaged()) return;

            var location = map.Playership.GetLocation();
            Quadrant quadrant = Quadrants.Get(map, location.Quadrant.X, location.Quadrant.Y);

            (new Output(StarTrekKGSettings.GetSetting<int>("ShieldsDownLevel"), StarTrekKGSettings.GetSetting<int>("LowEnergyLevel"))).PrintSector(quadrant, map); 

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
