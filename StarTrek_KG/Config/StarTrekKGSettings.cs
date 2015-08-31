using System.Collections.Generic;
using System.Configuration;
using System;
using System.Linq;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Config
{
    /// <summary>
    /// The purpose of this class is to allow adjustment of application strings and settings in a friendly format.  No database required.
    /// This, however, may change if the game ever scales up to multiplayer, in which case, just add on an IConfiguration interface, and make
    /// a data class implement it, and either switch to it, or allow switching.
    /// </summary>
    public class StarTrekKGSettings: ConfigurationSection, IStarTrekKGSettings
    {
        public StarTrekKGSettings Get { get; set; }
        public StarTrekKGSettings GetConfig()
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
            catch (Exception)
            {
                //todo: output a message to the user
                throw;
            }

            return settings ?? new StarTrekKGSettings();
        }

        #region Collections in Config file

        [ConfigurationProperty("StarSystems")]
        [ConfigurationCollection(typeof(Names), AddItemName = "StarSystem")]
        public Names StarSystems => (Names)this["StarSystems"];

        [ConfigurationProperty("ConsoleText")]
        [ConfigurationCollection(typeof(NameValues), AddItemName = "Text")]
        public NameValues ConsoleText => (NameValues)this["ConsoleText"];

        [ConfigurationProperty("Factions")]
        [ConfigurationCollection(typeof(Factions), AddItemName = "Faction")]
        public Factions Factions => (Factions)this["Factions"];

        [ConfigurationProperty("GameSettings")]
        [ConfigurationCollection(typeof(NameValues), AddItemName = "add")]
        public NameValues GameSettings => (NameValues)this["GameSettings"];

        [ConfigurationProperty("Menus")]
        [ConfigurationCollection(typeof(Menus), AddItemName = "MenuElement")]
        public Menus Menus => (Menus)this["Menus"];

        #endregion

        #region Helper Methods

        #region Factions
        public List<string> FactionShips(FactionName faction) //todo: this is called multiple times, do we need to reload file?
        {
            this.Reset();

            Faction factionElement = this.Get.Factions[faction.ToString()];

            FactionShips factionShips = factionElement.FactionShips;
            var shipNames = (from RegistryNameTypeClass shipElement in factionShips select shipElement.name.Trim()).ToList();
            return shipNames;
        }

        public Faction FactionDetails(FactionName faction)
        {
            this.Reset();

            var factionElement = this.Get.Factions[faction.ToString()];

            return factionElement;
        }

        public List<FactionThreat> GetThreats(FactionName faction)
        {
            if (faction == null)
            {
                throw new GameException("null faction");
            }

            this.Reset();

            FactionThreats factionThreatCollection = this.Get.Factions[faction.ToString()].FactionThreats;

            IEnumerable<SeverityValueTranslation> threatElements = (from SeverityValueTranslation threatElement in factionThreatCollection select threatElement).ToList();

            var threats = threatElements.Select(factionThreat => new FactionThreat(factionThreat.value, factionThreat.translation)) .ToList();

            return threats;
        }

        #endregion

        #region Menus

        public IEnumerable<string> GetMenuItems(string menuName) //todo: this is called multiple times, do we need to reload file?
        {
            this.Reset();

            MenuElement menuElement = this.Get.Menus[menuName];

            MenuItems menuItems = menuElement.MenuItems;

            //todo: this needs to return a list of MenuItems
            List<string> menuNames = (from RegistryNameTypeClass menuItemElement in menuItems select menuItemElement.name.Trim()).ToList();
            return menuNames;
        }


        #endregion

        public List<string> GetStarSystems()
        {
            this.Reset();

            var systems = this.Get.StarSystems;
            var systemNames = (from Name starSystem in systems select starSystem.name).ToList();
            return systemNames;
        }

        public string GetText(string textToGet, string textToGet2)
        {
            this.Reset();
            return this.GetText(textToGet) + this.GetText(textToGet2);
        }

        public string GetText(string name)
        {
            this.Reset();

            NameValue element = this.Get.ConsoleText[name];

            var setting = this.CheckAndCastValue<string>(name, element, true);

            return setting;
        }

        public T GetSetting<T>(string name)
        {
            this.Reset();

            NameValue element = this.Get.GameSettings[name];

            var setting = this.CheckAndCastValue<T>(name, element, true);

            return setting;
        }

        public string Setting(string name)
        {
            return this.GetSetting<string>(name);
        }

        #endregion

        public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false)
        {
            T setting;
            if (element != null)
            {
                if(whiteSpaceIsOk)
                {
                    if (element.value == null)
                    {
                        throw new ConfigurationErrorsException($"Unable to retrieve value for: {name} because it is null.");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(element.value))
                    {
                        throw new ConfigurationErrorsException($"Unable to retrieve value for: {name} because it is null or empty.");
                    }
                }
            }
            else
            {
                throw new ConfigurationErrorsException($"Unable to retrieve '{name}' config element.  Does it exist? Is it misspelled? (setting name is case sensitive)");
            }

            try
            {
                //user is passing in the data type (T), so typecast it here to the provided type so they don't have to.
                setting = (T) Convert.ChangeType(element.value, typeof (T));
            }
            catch (Exception)
            {
                throw new ConfigurationErrorsException( $"(DataType Error) Unable to cast config setting {name}. Check to make sure setting is filled out correctly.");
            }
            return setting;
        }

        public void Reset()
        {
            if (this.Get == null)
            {
                this.Get = this.GetConfig();
            }
        }
    }
}