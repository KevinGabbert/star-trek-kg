using StarTrek_KG.Config;
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

        //todo: get rid of that second, stupid parameter.
        public SectorDef(SectorItem sectorItem)
        {
            Setup_Playfield_Constants();

            this.Sector = new Sector(new LocationDef(null, new Coordinate((Utility.Random).Next(Constants.SECTOR_MAX), (Utility.Random).Next(Constants.SECTOR_MAX))));
            this.Item = sectorItem;
        }

        private static void Setup_Playfield_Constants()
        {
            Constants.SECTOR_MIN = AppConfig.Setting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = AppConfig.Setting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = AppConfig.Setting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = AppConfig.Setting<int>("QuadrantMax");
        }

        public SectorDef(LocationDef location, SectorItem sectorItem)
        {
            SectorDef.Setup_Playfield_Constants();

            string sectorSetupError = StarTrekKGSettings.GetText("SectorDefSetupError");

            if (location.Sector.X < Constants.SECTOR_MIN)
            {
                throw new GameConfigException(sectorSetupError + " Sector x < " + Constants.SECTOR_MIN);
            }

            if (location.Sector.X > Constants.SECTOR_MAX)
            {
                throw new GameConfigException(sectorSetupError + " Sector x > " + Constants.SECTOR_MAX);
            }

            if (location.Sector.Y < Constants.SECTOR_MIN)
            {
                throw new GameConfigException(sectorSetupError + "Sector y < " + Constants.SECTOR_MIN);
            }

            if (location.Sector.Y > Constants.SECTOR_MAX)
            {
                throw new GameConfigException(sectorSetupError + "Sector y > " + Constants.SECTOR_MAX);
            }

            this.Sector = new Sector(new LocationDef(location.Quadrant, new Coordinate(location.Sector.X, location.Sector.Y)));
            this.Item = sectorItem;
            this.QuadrantDef = location.Quadrant;
        }
    }
}
