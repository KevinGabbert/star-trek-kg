﻿using NUnit.Framework;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class PhaserTests
    {
        private Map _testMap = (new Map(new GameConfig
        {
            Initialize = true,
            GenerateMap = true,
            UseAppConfigSectorDefs = false,
            SectorDefs = new SectorDefs
            {
                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 1)), SectorItem.Friendly),
                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 3)), SectorItem.Hostile)
            },
            AddStars = false
        })); 

        [Test]
        public void HitThatDestroys()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 6)), SectorItem.Hostile)
                    },
                AddStars = false
            }));

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = _testMap.Quadrants.GetActive();

            Assert.AreEqual(1, activeQuadrant.Hostiles.Count);

            //Verify ship's location
            Assert.AreEqual(2, activeQuadrant.Hostiles[0].Sector.X);
            Assert.AreEqual(6, activeQuadrant.Hostiles[0].Sector.Y);

            //verify position on map.
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[22].Item);

            //set its energy
            Shields.For(activeQuadrant.Hostiles[0]).Energy = 50;

            //todo: verify firing ship's starting energy.

            //This action will hit every single hostile in the quadrant
            Phasers.For(_testMap.Playership).Fire(89.6829); //due to the distance between the 2 ships, this is how much power it takes to knock the hostile's shield level of 50 down to nothing.

            //todo: verify energy subtracted from firing ship.

            //in space. no one can hear you scream.
            Assert.AreEqual(0, activeQuadrant.Hostiles.Count);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[22].Item);
        }

        [Ignore]
        [Test]
        public void FirePhasersFromConsole()
        {
            //todo: a mock needed for this.  One that taps into the appropriate event.
            //Phasers.For(_testMap.Playership).Controls(_testMap);
        }

        public void HitThatWounds()
        {
            //_testMap.Quadrants.GetHostile(0).Shields = 20

            //todo: ensure that baddie has less than 50 (from config?)
            Phasers.For(_testMap.Playership).Fire(50);
        }

        public void HitThatWoundsMultipleHostiles()
        {
            //add a hostile before starting test
        }
    }
}
