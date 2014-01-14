using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class SectorDef
    {
        #region Properties

            public Coordinate QuadrantDef { get; set; }
            public Sector Sector { get; set; }

            public ISectorObject Object { get; set; }
            public SectorItem Item { get; set; }

        #endregion

        public SectorDef(SectorItem sectorItem)
        {
            this.Sector = new Sector(new LocationDef(Coordinate.GetRandom(), Coordinate.GetRandom()));

            //todo: if this quadrant already has a starbase, then don't assign.
            //check this.QuadrantDef.  If starbase exists, and sectorItem is a starbase, then assign empty instead.
            this.Item = sectorItem;
        }

        public SectorDef(LocationDef location, SectorItem sectorItem)
        {

            string sectorSetupError = (new StarTrekKGSettings()).GetText("SectorDefSetupError");

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
