using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class SystemsCascadePowerTransferTests
    {
        private Game _game;

        [SetUp]
        public void SetUp()
        {
            var setup = new SetupOptions
            {
                Initialize = true,
                IsSystemsCascadeMode = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                AddEnergyAnomalies = false,
                SystemsCascadeDestinationDistance = 5,
                SystemsCascadeGraceTurns = 3,
                SystemsCascadeEscalationChancePercent = 25,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
                }
            };

            _game = new Game(new StarTrekKGSettings(), setup);
            _game.Interact.Output.Clear();
        }

        [Test]
        public void TryHandleSystemsCascadeCommand_NotInMode_ReturnsFalse()
        {
            var normal = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
                }
            });

            var handled = normal.TryHandleSystemsCascadeCommand("pwr status", out var output);

            Assert.False(handled);
            Assert.IsNull(output);
        }

        [Test]
        public void TryHandleSystemsCascadeCommand_PwrHelp_ReturnsCommandDetails()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("pwr help", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("Systems Cascade Power Routing")));
            Assert.True(output.Any(line => line.Contains("pwr transfer [amount] [from] [to]")));
        }

        [Test]
        public void TryHandleSystemsCascadeCommand_PwrStatus_ReturnsPowerAllocation()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("pwr status", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("--- Power Allocation ---")));
            Assert.True(output.Any(line => line.Contains("srs: 100")));
            Assert.True(output.Any(line => line.Contains("she: 100")));
        }

        [Test]
        public void TryHandleSystemsCascadeCommand_CascadeStatus_ReturnsMissionStatus()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("cascade status", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("--- Systems Cascade Status ---")));
            Assert.True(output.Any(line => line.Contains("Destination Sector:")));
        }

        [Test]
        public void PwrTransfer_Valid_UpdatesPowerLevels()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 20 she srs", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("Transferred 20 power from she to srs.")));
            Assert.True(output.Any(line => line.Contains("she: 80")));
            Assert.True(output.Any(line => line.Contains("srs: 120")));
        }

        [Test]
        public void PwrTransfer_UnknownSystem_ShowsError()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 20 xxx srs", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("Unknown system.")));
        }

        [Test]
        public void PwrTransfer_InsufficientPower_ShowsError()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 1000 she srs", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("does not have enough allocated power")));
        }

        [Test]
        public void PwrTransfer_SameSourceDestination_ShowsError()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 20 she she", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("Source and destination systems must be different.")));
        }

        [Test]
        public void PwrTransfer_ZeroAmount_ShowsError()
        {
            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 0 she srs", out var output);

            Assert.True(handled);
            Assert.IsNotNull(output);
            Assert.True(output.Any(line => line.Contains("Power transfer amount must be greater than zero.")));
        }

        [Test]
        public void PwrTransfer_ToSrs_RepairsOneNoiseLine()
        {
            _game.TriggerSystemsCascadeFromAnomaly("&");
            Assert.AreEqual(2, _game.GetSystemsCascadeSrsNoiseLines());

            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 20 she srs", out _);

            Assert.True(handled);
            Assert.AreEqual(1, _game.GetSystemsCascadeSrsNoiseLines());
        }

        [Test]
        public void PwrTransfer_ToCrs_RepairsOneNoiseLine()
        {
            _game.TriggerSystemsCascadeFromAnomaly("%");
            Assert.AreEqual(2, _game.GetSystemsCascadeCrsNoiseLines());

            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 20 she crs", out _);

            Assert.True(handled);
            Assert.AreEqual(1, _game.GetSystemsCascadeCrsNoiseLines());
        }

        [Test]
        public void PwrTransfer_ToLrs_ReducesBrownoutLevel()
        {
            _game.TriggerSystemsCascadeFromAnomaly("~");
            Assert.AreEqual(20, _game.GetSystemsCascadeLrsNoiseLevel());

            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 20 she lrs", out _);

            Assert.True(handled);
            Assert.AreEqual(0, _game.GetSystemsCascadeLrsNoiseLevel());
        }

        [Test]
        public void PwrTransfer_ToSubsystem_RepairsDamageByOne()
        {
            var ship = _game.Map.Playership;
            Navigation.For(ship).Damage = 2;

            var handled = _game.TryHandleSystemsCascadeCommand("pwr transfer 20 she nav", out _);

            Assert.True(handled);
            Assert.AreEqual(1, Navigation.For(ship).Damage);
        }
    }
}
