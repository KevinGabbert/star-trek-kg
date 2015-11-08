using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;

namespace StarTrek_KG.Playfield
{
    public class SectorDef
    {
        #region Properties

            public Coordinate RegionDef { get; set; }
            public Sector Sector { get; set; }

            public ISectorObject Object { get; set; }
            public SectorItem Item { get; set; }

        #endregion

        public SectorDef(SectorItem sectorItem)
        {
            this.Sector = new Sector(new LocationDef(Coordinate.GetRandom(), Coordinate.GetRandom()));

            //todo: if this Region already has a starbase, then don't assign.
            //check this.RegionDef.  If starbase exists, and sectorItem is a starbase, then assign empty instead.
            this.Item = sectorItem;
        }

        public SectorDef(LocationDef location, SectorItem sectorItem)
        {

            string sectorSetupError = (new StarTrekKGSettings()).GetText("SectorDefSetupError");

            if (location.Sector.X < DEFAULTS.SECTOR_MIN)
            {
                throw new GameConfigException(sectorSetupError + " Sector x < " + DEFAULTS.SECTOR_MIN);
            }

            if (location.Sector.X > DEFAULTS.SECTOR_MAX)
            {
                throw new GameConfigException(sectorSetupError + " Sector x > " + DEFAULTS.SECTOR_MAX);
            }

            if (location.Sector.Y < DEFAULTS.SECTOR_MIN)
            {
                throw new GameConfigException(sectorSetupError + "Sector y < " + DEFAULTS.SECTOR_MIN);
            }

            if (location.Sector.Y > DEFAULTS.SECTOR_MAX)
            {
                throw new GameConfigException(sectorSetupError + "Sector y > " + DEFAULTS.SECTOR_MAX);
            }

            this.Sector = new Sector(new LocationDef(location.Region, new Coordinate(location.Sector.X, location.Sector.Y)));
            this.Item = sectorItem;
            this.RegionDef = location.Region;
        }
    }
}
