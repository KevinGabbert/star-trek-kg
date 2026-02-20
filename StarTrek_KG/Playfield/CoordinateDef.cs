using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;

namespace StarTrek_KG.Playfield
{
    public class CoordinateDef
    {
        #region Properties

            public Point SectorDef { get; set; }
            public Coordinate Coordinate { get; set; }

            public ICoordinateObject Object { get; set; }
            public CoordinateItem Item { get; set; }

        #endregion

        public CoordinateDef(CoordinateItem sectorItem)
        {
            this.Coordinate = new Coordinate(new LocationDef(Point.GetRandom(), Point.GetRandom()));

            //todo: if this Sector already has a starbase, then don't assign.
            //check this.SectorDef.  If starbase exists, and sectorItem is a starbase, then assign empty instead.
            this.Item = sectorItem;
        }

        public CoordinateDef(LocationDef location, CoordinateItem sectorItem)
        {
            string sectorSetupError = new StarTrekKGSettings().GetText("CoordinateDefSetupError");

            if (location.Coordinate.X < DEFAULTS.COORDINATE_MIN)
            {
                throw new GameConfigException($"{sectorSetupError} Coordinate x < {DEFAULTS.COORDINATE_MIN}");
            }

            if (location.Coordinate.X > DEFAULTS.COORDINATE_MAX)
            {
                throw new GameConfigException($"{sectorSetupError} Coordinate x > {DEFAULTS.COORDINATE_MAX}");
            }

            if (location.Coordinate.Y < DEFAULTS.COORDINATE_MIN)
            {
                throw new GameConfigException($"{sectorSetupError}Coordinate y < {DEFAULTS.COORDINATE_MIN}");
            }

            if (location.Coordinate.Y > DEFAULTS.COORDINATE_MAX)
            {
                throw new GameConfigException($"{sectorSetupError}Coordinate y > {DEFAULTS.COORDINATE_MAX}");
            }

            this.Coordinate = new Coordinate(new LocationDef(location.Sector, new Point(location.Coordinate.X, location.Coordinate.Y)));
            this.Item = sectorItem;
            this.SectorDef = location.Sector;
        }
    }
}
