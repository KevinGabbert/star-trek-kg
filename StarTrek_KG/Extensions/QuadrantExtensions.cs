using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Extensions
{
    public static class RegionExtensions
    {
        /// <summary>
        /// This will populate the entire Region with Nebulae.
        /// A sector that is of type: nebula can still have sector items in it, but you might not be able to see it on the SRS
        /// </summary>
        /// <param name="Region"></param>
        public static void TransformIntoNebulae(this Region Region)
        {
            //todo: later, find a way to name all contigious nebula sectors

            foreach (Sector sector in Region.Sectors)
            {
                sector.Type = SectorType.Nebula;
            }

            Region.Type = RegionType.Nebulae;
        }
    }
}
