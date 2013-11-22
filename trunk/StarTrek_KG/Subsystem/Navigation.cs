using System;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Navigation : SubSystem_Base, IMap
    {
        #region Properties

            public bool docked { get; set; } //todo: move this to ship
            public int MaxWarpFactor { get; set; }

            public Warp Warp { get; set; }
            public Movement Movement { get; set; }

        #endregion

        public Navigation(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.Navigation;

            this.Warp = new Warp();
            this.Movement = new Movement(map);
        }

        public override void OutputDamagedMessage()
        {
            Console.WriteLine("Warp engines damaged.");
        }
        public override void OutputRepairedMessage()
        {
            Output.WriteLine("Warp engines have been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        private void SetMaxWarpFactor()
        {
            this.MaxWarpFactor = (int)(0.2 + (Utility.Random).Next(9)); // / 10.0
            Console.WriteLine("Maximum warp factor: {0}", this.MaxWarpFactor);
        }

        public override void Controls(string command)
        {
            this.Controls();
        }

        public void Controls()
        {
            if(this.Damaged())
            {
                this.SetMaxWarpFactor();
            }

            double distance;
            string direction;

            if (this.Movement.InvalidCourseCheck(out direction)) return;
            if (this.Warp.InvalidWarpFactorCheck(this.MaxWarpFactor, out distance)) return;

            int lastQuadY;
            int lastQuadX;

            if (!Warp.EngageWarp(direction, distance, out lastQuadY, out lastQuadX, this.Map)) return;

            ShortRangeScan.For(this.Map.Playership).Controls(this.Map);

            //var lastPosition = Quadrants.Get(this.Map, lastQuadX, lastQuadY);
            //Sectors currentPosition = this.Map.GetCurrentSectors();  //fixme

            this.RepairOrTakeDamage(lastQuadX, lastQuadY );//,currentPosition
        }

        private void RepairOrTakeDamage(int lastQuadX, int lastQuadY) //, Sectors sectors
        {
            var thisShip = this.Map.Playership.GetLocation();

            docked = this.Map.IsDockingLocation(thisShip.Sector.Y, thisShip.Sector.X, this.Map.Quadrants.GetActive().Sectors);
            if (docked)
            {
                Output.WriteResourceLine("DockingMessageLowerShields");
                Shields.For(this.Map.Playership).Damage = 0;

                this.Map.Playership.RepairEverything();

                Output.DockSuccess(thisShip.name);
            }
            else
            {
                this.TakeAttackDamageOrRepair(this.Map, lastQuadY, lastQuadX);
            }
        }
        private void TakeAttackDamageOrRepair(Map map, int lastQuadY, int lastQuadX)
        {
            var thisShip = this.Map.Playership.GetLocation();
            var baddiesHangingAround = Quadrants.Get(map, thisShip.Quadrant.X, thisShip.Quadrant.Y).Hostiles.Count > 0;
            var stillInThisQuadrant = lastQuadX == thisShip.Quadrant.X && lastQuadY == thisShip.Quadrant.Y;

            if (baddiesHangingAround && stillInThisQuadrant)
            {
                map.Quadrants.ALLHostilesAttack(map);
            }
            else 
            {
                map.Playership.RepairSubsystem(map.Playership);
            }
        }

        public void Calculator(Map map)
        {
            var thisShip = this.Map.Playership.GetLocation();

            double quadX;
            double quadY;

            Console.WriteLine(this.Map.Playership.Name + " located in quadrant [{0},{1}].", (thisShip.Quadrant.X),
                              (thisShip.Quadrant.Y));

            if (!Command.PromptUser("Enter destination quadrant X (1--8): ", out quadX)
                || quadX < 1 || quadX > Constants.QUADRANT_MAX)
            {
                Output.WriteLine("Invalid X coordinate.");
                return;
            }

            if (!Command.PromptUser("Enter destination quadrant Y (1--8): ", out quadY)
                || quadY < 1 || quadY > Constants.QUADRANT_MAX)
            {
                Output.WriteLine("Invalid Y coordinate.");
                return;
            }

            Console.WriteLine();
            var qx = ((int)(quadX)) - 1;
            var qy = ((int)(quadY)) - 1;
            if (qx == thisShip.Quadrant.X && qy == thisShip.Quadrant.Y)
            {
                Console.WriteLine("That is the current location of the " + this.Map.Playership.Name + ".");
                return;
            }

            Console.WriteLine("Direction: {0:#.##}", Map.ComputeDirection(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy));
            Console.WriteLine("Distance:  {0:##.##}", Map.Distance(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy));
        }

        public new static Navigation For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Navigation). Add a Friendly to your GameConfig"); //todo: make this a custom exception
            }

            return (Navigation)ship.Subsystems.Single(s => s.Type == SubsystemType.Navigation);
        }


        ////temporary
        //public static Location GetLocation(Ship ship)
        //{
        //    Navigation returnVal = Navigation.For(ship);
        //    var location = new Location(returnVal.Map, returnVal.sectorX, returnVal.sectorY);

        //    location.Quadrant.X = returnVal.quadrantX;
        //    location.Quadrant.Y = returnVal.quadrantY;

        //    return location; //returnVal.Location;
        //}
    }
}
