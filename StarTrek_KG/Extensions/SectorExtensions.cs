using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Extensions
{
    public static class SectorExtensions
    {
        /// <summary>
        /// This will populate the entire Sector with Nebulae.
        /// A sector that is of type: nebula can still have sector items in it, but you might not be able to see it on the SRS
        /// </summary>
        /// <param name="Sector"></param>
        public static void TransformIntoNebulae(this Sector Sector)
        {
            //todo: later, find a way to name all contigious nebula sectors

            foreach (Coordinate sector in Sector.Coordinates)
            {
                sector.Type = CoordinateType.Nebula;
            }

            Sector.Type = SectorType.Nebulae;
        }
    }
}
