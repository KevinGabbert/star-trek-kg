using System.Configuration;

namespace StarTrek_KG.Config
{
    public class StarTrekKGSettings: ConfigurationSection
    {
        public static StarTrekKGSettings GetConfig()
        {
            var settings = (StarTrekKGSettings)ConfigurationManager.GetSection("StarTrekKGSettings");
            return settings ?? new StarTrekKGSettings();
        }

        [ConfigurationProperty("StarSystems")]
        [ConfigurationCollection(typeof(StarSystems), AddItemName = "StarSystem")]
        public StarSystems StarSystems
        {
            get
            {
                return (StarSystems)this["StarSystems"];
            }
        }

        [ConfigurationProperty("ConsoleText")]
        [ConfigurationCollection(typeof(ConsoleText), AddItemName = "Text")]
        public ConsoleText ConsoleText
        {
            get
            {
                return (ConsoleText)this["ConsoleText"];
            }
        }
    }
}
