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
            var settings = new StarTrekKGSettings();
            string sectorSetupError = settings.GetText("CoordinateDefSetupError");
            int coordinateMin = DEFAULTS.COORDINATE_MIN;
            int coordinateMax = DEFAULTS.COORDINATE_MAX;
            if (coordinateMax <= coordinateMin)
            {
                coordinateMin = settings.GetSetting<int>("COORDINATE_MIN");
                coordinateMax = settings.GetSetting<int>("COORDINATE_MAX");
            }

            if (location.Coordinate.X < coordinateMin)
            {
                throw new GameConfigException($"{sectorSetupError} Coordinate x < {coordinateMin}");
            }

            if (location.Coordinate.X >= coordinateMax)
            {
                throw new GameConfigException($"{sectorSetupError} Coordinate x >= {coordinateMax}");
            }

            if (location.Coordinate.Y < coordinateMin)
            {
                throw new GameConfigException($"{sectorSetupError}Coordinate y < {coordinateMin}");
            }

            if (location.Coordinate.Y >= coordinateMax)
            {
                throw new GameConfigException($"{sectorSetupError}Coordinate y >= {coordinateMax}");
            }

            this.Coordinate = new Coordinate(new LocationDef(location.Sector, new Point(location.Coordinate.X, location.Coordinate.Y)));
            this.Item = sectorItem;
            this.SectorDef = location.Sector;
        }
    }
}
