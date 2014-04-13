using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Types
{
    public class IRSResult : IScanResult
    {
        public Coordinate Coordinate { get; set; }
        public ISectorObject Object { get; set; }
        public SectorItem Item { get; set; }
        public bool MyLocation { get; set; }
        public bool GalacticBarrier { get; set; }
        public bool Unknown { get; set; }
        public string Name { get; set; }
        public string RegionName { get; set; }

        public override string ToString()
        {
            string returnVal;

            switch (this.Item)
            {
                case SectorItem.Empty:
                    returnVal = "Empty";
                    break;

                case SectorItem.FriendlyShip:
                case SectorItem.HostileShip:
                case SectorItem.Star:

                    returnVal = this.Object != null ? this.Object.Name : "SectorObject Error";

                    break;

                case SectorItem.Starbase:
                    returnVal = "Starbase"; //todo:  When starbases have names then combine with above
                    break;

                case SectorItem.PlayerShip:
                    returnVal = "<This Ship>";
                    break;

                case SectorItem.Debug:
                    returnVal = "X";
                    break;

                default:
                    returnVal = "Unknown";
                    break;
            }

            return returnVal;
        }
    }
}
