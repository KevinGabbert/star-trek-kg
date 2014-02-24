﻿using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class ShortRangeScan : SubSystem_Base
    {
        public ShortRangeScan(Ship shipConnectedTo, Game game) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.ShortRangeScan;
        }

        public void Controls()
        {
            if (this.Damaged()) return;

            this.Game.Write.RenderSector(SectorScanType.ShortRange, this);
        }

        public static ShortRangeScan For(IShip ship)
        {
            return (ShortRangeScan)SubSystem_Base.For(ship, SubsystemType.ShortRangeScan);
        }
    }
}
