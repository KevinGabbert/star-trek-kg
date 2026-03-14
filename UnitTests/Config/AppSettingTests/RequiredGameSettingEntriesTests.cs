using NUnit.Framework;
using StarTrek_KG.Config;

namespace UnitTests.Config.AppSettingTests
{
    [TestFixture]
    public class RequiredGameSettingEntriesTests
    {
        private StarTrekKGSettings _config;

        [SetUp]
        public void SetUp()
        {
            _config = new StarTrekKGSettings();
            _config.Get = _config.GetConfig();
        }

        [Test]
        public void RequiredIntSettings_Exist_And_Are_Parseable()
        {
            var required = new[]
            {
                "energy",
                "repairEnergy",
                "photonTorpedoes",
                "timeRemaining",
                "starbases",
                "totalHostiles",
                "MaxWarpFactor",
                "LowEnergyLevel",
                "GalacticBarrierDamage",
                "HostileCollisionDamage",
                "StarbaseCollisionDamage",
                "HostileDestructionSplashDamage",
                "HostileMaxRetreatAttempts",
                "BorgCubeCount",
                "BorgEnergy",
                "BorgShieldEnergy",
                "BorgAttackRange",
                "BorgPowerDrainPercent",
                "BorgPursuitDelayTurns",
                "BorgWormholeDelayTurns",
                "BorgBlackHoleLurePercent",
                "BorgWormholeLurePercent",
                "BorgInitialDamageableTurns",
                "BorgTorpedoDamage",
                "ZipBugCount",
                "ZipBugMaxTurnsPerSector",
                "ZipBugMaxEnergyAppearanceBonus",
                "ZipBugAdjacentEnergyBonus",
                "ZipBugFigureEightEnergyPerTurn",
                "DeuteriumSectorSpawnPercent",
                "DeuteriumSectorTotalMin",
                "DeuteriumSectorTotalMax",
                "DeuteriumCloudSectorPercent",
                "GaseousAnomalySectorPercent",
                "TemporalRiftSectorPercent",
                "SporeSectorPercent",
                "BlackHoleSectorPercent",
                "HostileOutpostSectorPercent",
                "BoardingShieldThreshold",
                "TSSLockStreakBonusPercent",
                "TSSLockStreakBonusCapPercent",
                "IRSPlusEnergyCost",
                "IRSPlusPlusEnergyCost",
                "IRSPlusPlusPlusEnergyCost",
                "GraviticMineDamage",
                "IRSHostileSystemsVisibleDistance"
            };

            foreach (var key in required)
            {
                Assert.DoesNotThrow(() => _config.GetSetting<int>(key), $"Missing or invalid int setting: {key}");
            }
        }

        [Test]
        public void RequiredBoolSettings_Exist_And_Are_Parseable()
        {
            var required = new[]
            {
                "KeepPlaying",
                "AllowMinesInStarbaseSectors",
                "enable-deuterium-sectors",
                "enable-gravitic-mines"
            };

            foreach (var key in required)
            {
                Assert.DoesNotThrow(() => _config.GetSetting<bool>(key), $"Missing or invalid bool setting: {key}");
            }
        }

        [Test]
        public void RequiredDoubleSettings_Exist_And_Are_Parseable()
        {
            var required = new[]
            {
                "DisruptorShotDeprecationLevel",
                "PhaserShotDeprecationRate",
                "DisruptorEnergyAdjustment",
                "PhaserEnergyAdjustment"
            };

            foreach (var key in required)
            {
                Assert.DoesNotThrow(() => _config.GetSetting<double>(key), $"Missing or invalid double setting: {key}");
            }
        }

        [Test]
        public void RequiredStringSettings_Exist_And_Are_Not_Empty()
        {
            var required = new[]
            {
                "PlayerShip",
                "Hostile",
                "CommandPrompt",
                "GalacticBarrierCRS",
                "HostileOutpostGlyph",
                "war-games-start-command",
                "ZipBugFigureEightGlyph",
                "ZipBugHostileGlyph",
                "ZipBugRevealGlyph"
            };

            foreach (var key in required)
            {
                string value = null;
                Assert.DoesNotThrow(() => value = _config.GetSetting<string>(key), $"Missing string setting: {key}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"String setting is empty: {key}");
            }
        }
    }
}
