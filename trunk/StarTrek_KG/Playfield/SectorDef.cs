using System;
using System.Configuration;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;

namespace StarTrek_KG.Playfield
{
    public class SectorDef
    {
        #region Properties

            public Coordinate QuadrantDef { get; set; }
            public Sector Sector { get; set; } 

            public SectorItem Item { get; set; }
        #endregion

        public SectorDef(SectorItem sectorItem)
        {
            Constants.SECTOR_MIN = Convert.ToInt32(ConfigurationManager.AppSettings["SECTOR_MIN"]);
            Constants.SECTOR_MAX = Convert.ToInt32(ConfigurationManager.AppSettings["SECTOR_MAX"]);

            Constants.QUADRANT_MIN = Convert.ToInt32(ConfigurationManager.AppSettings["QUADRANT_MIN"]);
            Constants.QUADRANT_MAX = Convert.ToInt32(ConfigurationManager.AppSettings["QuadrantMax"]);

            this.Sector = new Sector(new LocationDef(null, new Coordinate((Utility.Random).Next(Constants.SECTOR_MAX), (Utility.Random).Next(Constants.SECTOR_MAX))));
            this.Item = sectorItem;
        }

        public SectorDef(LocationDef location, SectorItem sectorItem)
        {
            if (location.Sector.X < Constants.SECTOR_MIN)
            {
                throw new GameConfigException("Error setting up Sector.  Sector x < " + Constants.SECTOR_MIN.ToString());
            }

            if (location.Sector.X > Constants.SECTOR_MAX)
            {
                throw new GameConfigException("Error setting up Sector.  Sector x > " + Constants.SECTOR_MAX.ToString());
            }

            if (location.Sector.Y < Constants.SECTOR_MIN)
            {
                throw new GameConfigException("Error setting up Sector.  Sector y < " + Constants.SECTOR_MIN.ToString());
            }

            if (location.Sector.Y > Constants.SECTOR_MAX)
            {
                throw new GameConfigException("Error setting up Sector.  Sector y > " + Constants.SECTOR_MAX.ToString());
            }

            this.Sector = new Sector(new LocationDef(location.Quadrant, new Coordinate(location.Sector.X, location.Sector.Y)));
            this.Item = sectorItem;
            this.QuadrantDef = location.Quadrant;
        }
    }
}
