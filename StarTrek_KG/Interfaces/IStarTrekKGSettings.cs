using System.Collections.Generic;
using System.Configuration;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IStarTrekKGSettings 
    {
        StarTrekKGSettings Get { get; set; }

        [ConfigurationProperty("StarSystems")]
        [ConfigurationCollection(typeof(Names), AddItemName = "StarSystem")]
        Names StarSystems { get; }

        [ConfigurationProperty("ConsoleText")]
        [ConfigurationCollection(typeof(NameValues), AddItemName = "Text")]
        NameValues ConsoleText { get; }

        [ConfigurationProperty("Factions")]
        [ConfigurationCollection(typeof(Factions), AddItemName = "Faction")]
        Factions Factions { get; }

        [ConfigurationProperty("GameSettings")]
        [ConfigurationCollection(typeof(NameValues), AddItemName = "add")]
        NameValues GameSettings { get; }

        new StarTrekKGSettings GetConfig();
        List<string> FactionShips(FactionName faction);
        List<FactionThreat> GetThreats(FactionName faction);
        List<string> GetStarSystems();
        string GetText(string textToGet, string textToGet2);
        string GetText(string name);
        T GetSetting<T>(string name);
        string Setting(string name);
        T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false);
        void Reset();
    }
}
