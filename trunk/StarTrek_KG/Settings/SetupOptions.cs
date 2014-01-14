using StarTrek_KG.Playfield;

namespace StarTrek_KG.Settings
{
    //we are making this object so that tests that require certain objects setup can have them set up individually.
    //docking tests dont work when starbases are included.
    //movement tests hit obstacles when they are setup automatically
    public class SetupOptions 
    {
        public bool Initialize { get; set; }

        private bool _addStars = true;
        public bool AddStars
        {
            get { return _addStars; }
            set { _addStars = value; }
        }

        /// <summary>
        /// The plan here is to be able to start the game by throwing in all objects required for game with XY values
        /// </summary>
        public SectorDefs SectorDefs { get; set; } 
    }
}
