using System.Configuration;
using System;
using StarTrek_KG.Config.Collections;

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
                //var x = settings.Factions["Vulcan"].FactionShips[0];
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
    }
}


namespace Brh.Web
{
    class RedirectConfigurationHandler : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection=true, IsKey=false, IsRequired=true)]
        public RedirectCollection Redirects
        {
            get
            {
                return base[""] as RedirectCollection;
            }

            set
            {
                base[""] = value;
            }
        }
    }

    class RedirectCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new RedirectElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RedirectElement)element).FilePattern;
        }

        protected override string ElementName
        {
            get
            {
                return "redirect";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return !String.IsNullOrEmpty(elementName) && elementName == "redirect";
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }


        public RedirectElement this[int index] {
            get
            {
                return base.BaseGet(index) as RedirectElement;
            }
        }
    }

    class RedirectElement : ConfigurationElement
    {
        [ConfigurationProperty("filePattern", IsRequired=true, IsKey=true)]
        public string FilePattern
        {
            get { return base["filePattern"] as string; }
            set { base["filePattern"] = value; }
        }

        [ConfigurationProperty("url", IsRequired=true, IsKey=false)]
        public string RedirectUrl
        {
            get { return base["url"] as string; }
            set { base["url"] = value; }
        }
    }
}