using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class WarGamesNaturalLanguageTests : TestClass_Base
    {
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            this.Game = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                IsWarGamesMode = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 1)), CoordinateItem.HostileShip)
                }
            });

            _interact = (Interaction)this.Game.Interact;
            _interact.Output.Clear();
        }

        [Test]
        public void WarGamesNaturalLanguage_AddPluralActors_AddsRequestedCount()
        {
            var sector = this.Game.Map.Playership.GetSector();
            var before = sector.Coordinates.Count(c => c.Item == CoordinateItem.Starbase);

            _interact.ReadAndOutput(this.Game.Map.Playership, "map", "add 3 starbases");

            var after = sector.Coordinates.Count(c => c.Item == CoordinateItem.Starbase);
            Assert.AreEqual(before + 3, after);
        }

        [Test]
        public void WarGamesNaturalLanguage_AddByFaction_AddsRequestedFactionShip()
        {
            var sector = this.Game.Map.Playership.GetSector();
            var before = sector.GetHostiles().Count;

            _interact.ReadAndOutput(this.Game.Map.Playership, "map", "add 1 klingon");

            var hostiles = sector.GetHostiles();
            Assert.AreEqual(before + 1, hostiles.Count);
            Assert.True(hostiles.Any(h => h.Faction == FactionName.Klingon));
        }

        [Test]
        public void WarGamesNaturalLanguage_AddAndDestroyByShipName_WorksAtTopLevel()
        {
            var sector = this.Game.Map.Playership.GetSector();
            var shipName = this.Game.Config.ShipNames(FactionName.Klingon).First();

            _interact.ReadAndOutput(this.Game.Map.Playership, "map", $"add {shipName}");
            Assert.True(sector.GetHostiles().Any(h => h.Name == shipName));

            _interact.ReadAndOutput(this.Game.Map.Playership, "map", $"destroy {shipName}");
            Assert.False(sector.GetHostiles().Any(h => h.Name == shipName));
        }
    }
}
