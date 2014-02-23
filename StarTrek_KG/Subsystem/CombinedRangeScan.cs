using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
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
            if (Damaged()) return;

            this.Game.Write.RenderSector(SectorScanType.CombinedRange, this);
        }

        public static CombinedRangeScan For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (CombinedRangeScan)."); //todo: reflect the name and refactor this to ISubsystem
            }

            return (CombinedRangeScan) ship.Subsystems.Single(s => s.Type == SubsystemType.CombinedRangeScan);
        }
    }
}
