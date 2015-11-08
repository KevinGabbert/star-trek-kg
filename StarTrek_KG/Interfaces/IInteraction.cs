﻿using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Output;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// Objects that implement this are intended to output various types of information.
    /// </summary>
    public interface IInteraction
    {
        List<string> SHIP_PANEL { get; }
        IStarTrekKGSettings Config { get; set; }
        IOutputMethod Output { get; set; }
        Subscriber Subscriber { get; }

        string CurrentPrompt { get; set; }

        bool OutputError { get; set; }

        void OutputStrings(IEnumerable<string> strings);
        void HighlightTextBW(bool on);
        
        List<string> Line(string stringToOutput);

        string GetFormattedConfigText(string configTextToWrite, object param1);
        string GetFormattedConfigText(string configTextToWrite, object param1, object param2);

        void FormattedConfigLine(string configTextToWrite, object param1);
        void FormattedConfigLine(string configTextToWrite, object param1, object param2);

        void ConfigText(string configTextName);

        void DebugLine(string stringToOutput);
        void Resource(string text);
        void ResourceLine(string text);
        void ResourceSingleLine(string text);
        void ResourceLine(string prependText, string text);
        void SingleLine(string stringToOutput);
        void WithNoEndCR(string stringToOutput);
        void DisplayPropertiesOf(object @object);

        void RenderRegionCounts(bool renderingMyLocation, int starbaseCount, int starCount, int hostileCount);
        string RenderRegionCounts(int starbaseCount, int starCount, int hostileCount);

        void RenderNebula(bool renderingMyLocation);
        void RenderUnscannedRegion(bool renderingMyLocation);
        List<string> RenderLRSData(IEnumerable<LRSResult> lrsData, IGame game);
        //IEnumerable<string> RenderIRSData(IEnumerable<IRSResult> irsResults, IGame game);
        //IEnumerable<string> RenderLRSWithNames(List<IScanResult> lrsData, IGame game);
        IEnumerable<string> RenderScanWithNames(ScanRenderType scanRenderType, string title, List<IScanResult> list, IGame game);

        void CreateCommandPanel();
        List<string> Panel(string panelHead, IEnumerable<string> strings);
        //string GetPanelHead(string shipName);
        List<string> ReadAndOutput(Ship playerShip, string mapText, IGame game, string userInput = null);

        void ResetPrompt();
        bool PromptUser(SubsystemType promptSubsystem, string promptDisplay, string promptMessage, out string value, Queue<string> queueToWriteTo, int subPromptLevel = 0);
        bool PromptUser(SubsystemType promptSubsystem, string promptMessage, out int value, int subPromptLevel = 0);
        bool PromptUserConsole(string promptMessage, out string value);
        bool PromptUserSubscriber(string promptMessage, out string value);

        List<string> EvalSubLevelCommand(IShip playerShip, string playerEnteredText, int promptLevel);

        void OutputConditionAndWarnings(Ship ship, int shieldsDownLevel);
        void RenderSectors(SectorScanType scanType, ISubsystem subsystem);
        string RenderCourse();

        //todo: these don't belong here.  move them

        //int TotalHostiles { get; set; }
        //int TimeRemaining { get; set; }

        //int Starbases { get; set; }
        //int Stardate { get; set; }

        string ShipHitMessage(IShip attacker, int attackingEnergy);
        string MisfireMessage(IShip attacker);

        void PrintMissionResult(Ship ship, bool starbasesAreHostile, int starbasesLeft);
        void PrintMission();
    }
}
