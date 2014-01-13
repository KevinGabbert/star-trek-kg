﻿using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    //todo: make feature where opposing ships can hack into your computer (and you, theirs) if shields are down
    public class Computer : SubSystem_Base
    {
        public static readonly string[] CONTROL_PANEL = {
                                                    "--- > Main Computer --------------",
                                                    "rec = Cumulative Galactic Record",
                                                    "sta = Status Report",
                                                    "tor = Photon Torpedo Calculator",
                                                    "bas = Starbase Calculator",
                                                    "nav = Navigation Calculator"
                                                };

        public Computer(Ship shipConnectedTo, Game game)
        {
            this.Game = game;

            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
            this.Type = SubsystemType.Computer;
            this.Damage = 0;
        }

        public override void OutputDamagedMessage()
        {
            this.Game.Write.Line("The main computer has been Damaged.");
        }
        public override void OutputRepairedMessage()
        {
            this.Game.Write.Line("The main computer has been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            this.Game.Write.Line("The Main Computer is malfunctioning.");
        }

        public override void Controls(string command)
        {
            if (Damaged()) return;

            var starship = this.ShipConnectedTo;

            switch (command.ToLower())
            {
                case "rec":
                    Computer.For(this.ShipConnectedTo).PrintGalacticRecord(this.Game.Map.Quadrants);
                    break;

                case "sta":

                    //todo: get a list of all baddie names in quadrant

                    Computer.For(this.ShipConnectedTo).PrintCurrentStatus(this.Game.Map, 
                                              this.Damage, 
                                              starship,
                                              this.ShipConnectedTo.GetQuadrant());
                    break;

                case "tor":
                    Torpedoes.For(this.ShipConnectedTo).Calculator(this.Game.Map);
                    break;

                case "bas":
                    this.Game.Map.StarbaseCalculator(this.ShipConnectedTo); 
                    break;

                case "nav":
                    Navigation.For(this.ShipConnectedTo).Calculator(this.Game.Map);
                    break;

                default:
                    this.Game.Write.Line("Invalid computer this.Write");
                    break;
            }
        }


        //output this as KeyValueCollection that the UI can display as it likes.

        public void PrintCurrentStatus(Map map, int computerDamage, Ship ship, Quadrant currentQuadrant)
        {
            //todo: completely redo this

            this.Game.Write.Console.WriteLine("");
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSTimeRemaining"), map.timeRemaining);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSHostilesRemaining"), map.Quadrants.GetHostileCount());
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSHostilesInQuadrant"), currentQuadrant.GetHostiles().Count);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSStarbases"), map.starbases);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSWarpEngineDamage"), Navigation.For(ship).Damage);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSSRSDamage"), ShortRangeScan.For(ship).Damage);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSLRSDamage"), LongRangeScan.For(ship).Damage);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSShieldsDamage"), Shields.For(ship).Damage);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSComputerDamage"), computerDamage);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSPhotonDamage"), Torpedoes.For(ship).Damage);
            this.Game.Write.Console.WriteLine(StarTrekKGSettings.GetText("CSPhaserDamage"), Phasers.For(ship).Damage);
            this.Game.Write.Console.WriteLine();

            //foreach (var badGuy in currentQuadrant.Hostiles)
            //{
            //    
            //}

            this.Game.Write.Console.WriteLine();

            //todo: Display all baddie names in quadrant when encountered.
        }


        //This needs to be output as an array, List<List>, or KeyValueCollection, and the grid needs to be generated by the UI app

        public void PrintGalacticRecord(List<Quadrant> quadrants)
        {
            this.Game.Write.Console.WriteLine();
            this.Game.Write.ResourceSingleLine("GalacticRecordLine");

            var myLocation = this.ShipConnectedTo.GetLocation();

            for (var quadrantLB = 0; quadrantLB < Constants.QUADRANT_MAX; quadrantLB++)
            {
                for (var quadrantUB = 0; quadrantUB < Constants.QUADRANT_MAX; quadrantUB++)
                {
                    this.Game.Write.WithNoEndCR(Constants.SCAN_SECTOR_DIVIDER);
                    int starbaseCount = -1;
                    int starCount = -1;
                    int hostileCount = -1;

                    var quadrant = Playfield.Quadrants.Get(quadrants, new Coordinate(quadrantUB, quadrantLB));
                    if (quadrant.Scanned)
                    {
                        starbaseCount = quadrant.GetStarbaseCount();
                        starCount = quadrant.GetStarCount();
                        hostileCount = quadrant.GetHostiles().Count();
                    }

                    bool renderingMyLocation = false;
                    
                    if(myLocation.Quadrant.Scanned)
                    {
                        renderingMyLocation = myLocation.Quadrant.X == quadrantUB && myLocation.Quadrant.Y == quadrantLB;
                    }

                    this.Game.Write.RenderQuadrantCounts(renderingMyLocation, starbaseCount, starCount, hostileCount);
                }

                this.Game.Write.SingleLine(Constants.SCAN_SECTOR_DIVIDER);
                this.Game.Write.ResourceSingleLine("GalacticRecordLine");
            }

            this.Game.Write.Console.WriteLine();
        }

        public new static Computer For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Computer).");
            }

            return (Computer)ship.Subsystems.Single(s => s.Type == SubsystemType.Computer); //todo: reflect the name and refactor this to ISubsystem
        }

        //public string GenerateControlPanel()
        //{
            //reflect over functions?
        //}
    }
}
