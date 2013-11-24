using System.Collections.Generic;
using System.Configuration;
using System;
using System.Linq;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config
{
    public class StarTrekKGSettings: ConfigurationSection
    {
        public static StarTrekKGSettings GetConfig()
        {
            StarTrekKGSettings settings;

            try
            {
                settings = (StarTrekKGSettings)ConfigurationManager.GetSection("StarTrekKGSettings");
            }
            catch (ConfigurationErrorsException cx)
            {
                if (cx.Message.Contains("Unrecognized element"))
                {
                    //do you have your StarTrekKGSettings hierarchy set up properly? 
                    //No you don't.  
                    //Go to the line provided and either fix it or add code to handle functionality.
                    //if development is done, then the error is likely a misspelling.
                    //todo: output a message to the user
                    throw;
                }
                else
                {
                     throw; //todo: output a message to the user
                }
            }
            catch (Exception ex)
            {
                //todo: output a message to the user
                throw;
            }

            return settings ?? new StarTrekKGSettings();
        }
        public static StarTrekKGSettings Get { get; set; }

        [ConfigurationProperty("StarSystems")]
        [ConfigurationCollection(typeof(Names), AddItemName = "StarSystem")]
        public Names StarSystems
        {
            get
            {
                return (Names)this["StarSystems"];
            }
        }

        [ConfigurationProperty("ConsoleText")]
        [ConfigurationCollection(typeof(NameValues), AddItemName = "Text")]
        public NameValues ConsoleText
        {
            get
            {
                return (NameValues)this["ConsoleText"];
            }
        }

        [ConfigurationProperty("Factions")]
        [ConfigurationCollection(typeof(Factions), AddItemName = "Faction")]
        public Factions Factions
        {
            get
            {
                return (Factions)this["Factions"];             
            }
        }

        public static List<string> GetShips(string faction)
        {
            if(StarTrekKGSettings.Get == null)
            {
                StarTrekKGSettings.Get = StarTrekKGSettings.GetConfig();
            }

            FactionShips factionShips = StarTrekKGSettings.Get.Factions[faction].FactionShips;
            var shipNames = (from NameElement shipElement in factionShips select shipElement.name).ToList();
            return shipNames;
        }

        public static List<string> GetStarSystems()
        {
            if (StarTrekKGSettings.Get == null)
            {
                StarTrekKGSettings.Get = StarTrekKGSettings.GetConfig();
            }

            var systems = StarTrekKGSettings.Get.StarSystems;
            var systemNames = (from NameElement starSystem in systems select starSystem.name).ToList();
            return systemNames;
        }

        public static string GetText(string textToGet)
        {
            StarTrekKGSettings.Reset();
            return StarTrekKGSettings.Get.ConsoleText[textToGet].value;
        }

        public static string GetText(string textToGet, string textToGet2)
        {
            StarTrekKGSettings.Reset();
            return StarTrekKGSettings.Get.ConsoleText[textToGet].value + StarTrekKGSettings.Get.ConsoleText[textToGet2].value;
        }

        private static void Reset()
        {
            if (StarTrekKGSettings.Get == null)
            {
                StarTrekKGSettings.Get = StarTrekKGSettings.GetConfig();
            }
        }
    }
}