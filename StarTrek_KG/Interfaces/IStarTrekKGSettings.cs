﻿using System.Collections.Generic;
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

        StarTrekKGSettings GetConfig();

        /// <summary>
        /// FactionShips look like this in the app.config: <FactionShip type="Cruiser" name="IKC B'iJik"/>
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        List<string> FactionShips(FactionName faction);

        /// <summary>
        /// Threats look like this in the app.config: <FactionThreat severity="normal" value="wej 'avwI' yInISQo'!" translation="Do not let your guard down!"/>
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        List<FactionThreat> GetThreats(FactionName faction);

        /// <summary>
        /// StarSystems look like this in the app.config: <StarSystem name="Bellerophon"/>
        /// </summary>
        /// <returns></returns>
        List<string> GetStarSystems();


        /// <summary>
        /// Text looks like this in the app.Config: <Text name="PhaserDamage" value="Long range scanner is damaged."/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetText(string name);
        string GetText(string textToGet, string textToGet2);


        /// <summary>
        /// Settings look like this in the app.config: <add name="DebugNoSetUpSectorsInRegion" value="No Sectors Set up in Region: "/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        T GetSetting<T>(string name);

        string Setting(string name);
        T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false);
        void Reset();
    }
}
