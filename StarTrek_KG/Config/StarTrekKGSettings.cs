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
        public static StarTrekKGSettings Get { get; set; }
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
                    //do you have your StarTrekKG Settings hierarchy set up properly? 
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

        #region Collections

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

        [ConfigurationProperty("GameSettings")]
        [ConfigurationCollection(typeof(NameValues), AddItemName = "add")]
        public NameValues GameSettings
        {
            get
            {
                return (NameValues)this["GameSettings"];
            }
        }

        #endregion


        #region Helper Methods

        public static List<string> GetShips(string faction)
        {
            StarTrekKGSettings.Reset();

            FactionShips factionShips = StarTrekKGSettings.Get.Factions[faction].FactionShips;
            var shipNames = (from NameElement shipElement in factionShips select shipElement.name).ToList();
            return shipNames;
        }

        public static List<string> GetStarSystems()
        {
            StarTrekKGSettings.Reset();

            var systems = StarTrekKGSettings.Get.StarSystems;
            var systemNames = (from NameElement starSystem in systems select starSystem.name).ToList();
            return systemNames;
        }

        public static string GetText(string textToGet, string textToGet2)
        {
            StarTrekKGSettings.Reset();
            return StarTrekKGSettings.GetText(textToGet) + StarTrekKGSettings.GetText(textToGet2);
        }

        public static string GetText(string name)
        {
            StarTrekKGSettings.Reset();

            NameValueElement element = StarTrekKGSettings.Get.ConsoleText[name];

            var setting = StarTrekKGSettings.CheckAndCastValue<string>(name, element, true);

            return setting;
        }

        public static T GetSetting<T>(string name)
        {
            StarTrekKGSettings.Reset();

            NameValueElement element = StarTrekKGSettings.Get.GameSettings[name];

            var setting = StarTrekKGSettings.CheckAndCastValue<T>(name, element, true);

            return setting;
        }

        #endregion


        private static T CheckAndCastValue<T>(string name, NameValueElement element, bool whiteSpaceIsOk = false)
        {
            T setting;
            if (element != null)
            {
                if(whiteSpaceIsOk)
                {
                    if (element.value == null)
                    {
                        throw new ConfigurationErrorsException("Unable to retrieve value for: " + name + " because it is null.");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(element.value))
                    {
                        throw new ConfigurationErrorsException("Unable to retrieve value for: " + name + " because it is null or empty.");
                    }
                }
            }
            else
            {
                throw new ConfigurationErrorsException("Unable to retrieve '" + name + "' config element.  Does it exist? Is it misspelled? (setting name is case sensitive)");
            }

            try
            {
                //user is passing in the data type (T), so typecast it here to the provided type so they don't have to.
                setting = (T) Convert.ChangeType(element.value, typeof (T));
            }
            catch (Exception)
            {
                throw new ConfigurationErrorsException(
                    string.Format(
                        "(DataType Error) Unable to cast config setting {0}. Check to make sure setting is filled out correctly.",
                        name));
            }
            return setting;
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