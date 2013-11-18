using System.Configuration;

namespace StarTrek_KG.Config
{
    public class StarTrekKGSettings: ConfigurationSection
    {
        public static StarTrekKGSettings GetConfig()
        {
            return (StarTrekKGSettings)ConfigurationManager.GetSection("StarTrekKGSettings") ?? new StarTrekKGSettings();
        }

        [ConfigurationProperty("StarSystems")]
        public StarSystems StarSystems
        {
            get
            {
                var o = this["StarSystems"];
                return o as StarSystems;
            }
        }

    }
}
