using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Extensions
{
    public static class QuadrantExtensions
    {
        /// <summary>
        /// This will populate the entire quadrant with Nebulae.
        /// A sector that is of type: nebula can still have sector items in it, but you might not be able to see it on the SRS
        /// </summary>
        /// <param name="quadrant"></param>
        public static void TransformIntoNebulae(this Quadrant quadrant)
        {
            //todo: later, find a way to name all contigious nebula sectors

            foreach (Sector sector in quadrant.Sectors)
            {
                sector.Type = SectorType.Nebula;
            }

            quadrant.Type = QuadrantType.Nebulae;
        }
    }
}
