using StarTrek_KG.Playfield;

namespace StarTrek_KG.Settings
{
    //we are making this object so that tests that require certain objects setup can have them set up individually.
    //docking tests dont work when starbases are included.
    //movement tests hit obstacles when they are setup automatically
    public class SetupOptions 
    {
        public bool Initialize { get; set; }
        public bool AddStars { get; set; } = true;
        public bool AddNebulae { get; set; } = true;
        public bool AddDeuterium { get; set; } = true;
        public bool AddGraviticMines { get; set; } = true;

        /// <summary>
        /// The plan here is to be able to start the game by throwing in all objects required for game with XY values
        /// </summary>
        public CoordinateDefs CoordinateDefs { get; set; } 
    }
}
