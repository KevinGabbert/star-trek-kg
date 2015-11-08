using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

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
        public string RegionName { get; set; }

        public string ToScanString()
        {
            string returnVal;

            switch (this.Item)
            {
                case SectorItem.Empty:
                    returnVal = "Empty Space"; //todo: resource this out
                    break;

                case SectorItem.FriendlyShip:
                case SectorItem.HostileShip:
                case SectorItem.Star:

                    returnVal = this.Object != null ? this.Object.Name : DEFAULTS.SECTOR_INDICATOR + " Error"; //todo: resource this out

                    break;

                case SectorItem.Starbase:
                    returnVal = "Starbase"; //todo:  When starbases have names then combine with above
                    break;

                case SectorItem.PlayerShip:
                    returnVal = "<This Ship>"; //todo: identify ship
                    break;

                case SectorItem.Debug:
                    returnVal = "X";
                    break;

                default:
                    returnVal = "Unknown"; //todo: resource this out
                    break;
            }

            return returnVal;
        }
    }
}
