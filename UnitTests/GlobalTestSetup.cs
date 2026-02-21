using System;
using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace UnitTests
{
    [SetUpFixture]
    public class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void ConfigureAppConfig()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "app.config");
            if (!File.Exists(configPath))
            {
                return;
            }

            AppContext.SetData("APP_CONFIG_FILE", configPath);
            ConfigurationManager.RefreshSection("StarTrekKGSettings");
            ConfigurationManager.RefreshSection("Commands");
        }
    }
}
