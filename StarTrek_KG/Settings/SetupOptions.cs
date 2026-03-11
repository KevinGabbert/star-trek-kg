using StarTrek_KG.Playfield;
using System.Collections.Generic;

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
        public bool StrictDeterministic { get; set; } = false;
        public bool IsWarGamesMode { get; set; } = false;
        public bool IsSystemsCascadeMode { get; set; } = false;
        public string OpeningPictureKey { get; set; }
        public bool AddEnergyAnomalies { get; set; } = false;
        public int SystemsCascadeAnomalyDensityPercent { get; set; } = 10;
        public int SystemsCascadeDestinationDistance { get; set; } = 5;
        public int SystemsCascadeEmergencyPowerTurns { get; set; } = 6;
        public int SystemsCascadeEmergencyPowerPerTurn { get; set; } = 120;
        public int SystemsCascadeNebulaDeuteriumMultiplier { get; set; } = 3;
        public int SystemsCascadeEscalationChancePercent { get; set; } = 25;
        public int SystemsCascadeGraceTurns { get; set; } = 3;
        public int SystemsCascadeAnomalyEnergyHit { get; set; } = 60;
        public int SystemsCascadeAnomalyScannerNoisePerHit { get; set; } = 2;
        public int SystemsCascadeAnomalyLrsNoisePerHit { get; set; } = 20;
        public List<string> SystemsCascadeIntroLines { get; set; }
        public string SystemsCascadePowerHelpText { get; set; }

        /// <summary>
        /// The plan here is to be able to start the game by throwing in all objects required for game with XY values
        /// </summary>
        public CoordinateDefs CoordinateDefs { get; set; } 
    }
}
