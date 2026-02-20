using StarTrek_KG.Playfield;

namespace StarTrek_KG.Extensions
{
    public static class CoordinateDefExtensions
    {
        /// <summary>
        /// The only way we can use this is if Sectors get randomly assigned at this point.
        /// </summary>
        /// <param name="sectorDefs"></param>
        /// <param name="Sectors"> </param>
        /// <returns></returns>
        public static Coordinates ToCoordinates(this CoordinateDefs sectorDefs, Sectors Sectors)
        {
            var newSectors = new Coordinates();

            foreach (var sectorDef in sectorDefs)
            {
                Coordinates.SetupNewSector(sectorDef, newSectors, Sectors);
            }

            return newSectors;
        }
    }
}
