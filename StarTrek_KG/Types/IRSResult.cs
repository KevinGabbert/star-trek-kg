using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace StarTrek_KG.Types
{
    public class IRSResult : IScanResult
    {
        public Point Point { get; set; }
        public ICoordinateObject Object { get; set; }
        public CoordinateItem Item { get; set; }

        public bool MyLocation { get; set; }
        public bool GalacticBarrier { get; set; }
        public bool Unknown { get; set; }
        public string SectorName { get; set; }

        public string ToScanString()
        {
            string returnVal;

            switch (this.Item)
            {
                case CoordinateItem.Empty:
                    returnVal = "Empty Space"; //todo: resource this out
                    break;

                case CoordinateItem.FriendlyShip:
                case CoordinateItem.HostileShip:
                case CoordinateItem.Star:

                    returnVal = this.Object != null ? this.Object.Name : DEFAULTS.SECTOR_INDICATOR + " Error"; //todo: resource this out

                    break;

                case CoordinateItem.Starbase:
                    returnVal = "Starbase"; //todo:  When starbases have names then combine with above
                    break;

                case CoordinateItem.PlayerShip:
                    returnVal = "<This Ship>"; //todo: identify ship
                    break;

                case CoordinateItem.Debug:
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
