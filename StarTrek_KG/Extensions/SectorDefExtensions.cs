using StarTrek_KG.Playfield;

namespace StarTrek_KG.Extensions
{
    public static class SectorDefExtensions
    {
        /// <summary>
        /// The only way we can use this is if Regions get randomly assigned at this point.
        /// </summary>
        /// <param name="sectorDefs"></param>
        /// <param name="Regions"> </param>
        /// <returns></returns>
        public static Sectors ToSectors(this SectorDefs sectorDefs, Regions Regions)
        {
            var newSectors = new Sectors();

            foreach (var sectorDef in sectorDefs)
            {
                Sectors.SetupNewSector(sectorDef, newSectors, Regions);
            }

            return newSectors;
        }
    }
}
