using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class GameEventLogTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
        }

        [Test]
        public void NewGame_Resets_Previous_Game_Log_File()
        {
            var marker = "TEST-MARKER-" + Guid.NewGuid().ToString("N");
            var game1 = this.CreateGame();
            game1.AppendGameEventLog(marker);

            var path = game1.GameEventLogPath;
            Assert.IsTrue(File.Exists(path));
            var firstRead = File.ReadAllText(path);
            Assert.IsTrue(firstRead.Contains(marker));

            var game2 = this.CreateGame();
            var secondRead = File.ReadAllText(game2.GameEventLogPath);

            Assert.IsFalse(secondRead.Contains(marker), "Previous game marker should be removed when a new game starts.");
        }

        [Test]
        public void DebugDlog_Prints_Latest_Game_Log_Lines()
        {
            var marker = "DLOG-MARKER-" + Guid.NewGuid().ToString("N");
            var game = this.CreateGame();
            game.AppendGameEventLog(marker);

            var output = Debug.For(game.Map.Playership).Controls("dlog");

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Any(line => line.Contains("Latest Game Event Log")));
            Assert.IsTrue(output.Any(line => line.Contains(marker)));
        }

        private Game CreateGame()
        {
            var config = new StarTrekKGSettings();
            return new Game(config, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,1)), CoordinateItem.HostileShip)
                }
            });
        }
    }
}
