using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using Console = StarTrek_KG.Utility.Console;

namespace StarTrek_KG.Interfaces
{
    public interface IOutputWrite
    {
        List<string> ACTIVITY_PANEL { get; set; }
        IStarTrekKGSettings Config { get; set; }
        Console Console { get; set; }
        List<string> OutputQueue { get; set; }
        IOutput Output { get; set; }

        int TotalHostiles { get; set; }
        int TimeRemaining { get; set; }
        int Starbases { get; set; }
        int Stardate { get; set; }

        void PrintCommandResult(Ship ship, bool starbasesAreHostile, int starbasesLeft);
        void PrintMission();
        void Strings(IEnumerable<string> strings);
        void HighlightTextBW(bool on);
        
        void Line(string stringToOutput);

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
        List<string> RenderLRSData(IEnumerable<LRSResult> lrsData, Game game);
        //IEnumerable<string> RenderIRSData(IEnumerable<IRSResult> irsResults, Game game);

        //IEnumerable<string> RenderLRSWithNames(List<IScanResult> lrsData, Game game);
        IEnumerable<string> RenderScanWithNames(ScanRenderType scanRenderType, string title, List<IScanResult> list, Game game);

        void CreateCommandPanel();
        void ReadAndOutput(Ship playerShip, string mapText, Game game, string command = null);
        void Panel(string panelHead, IEnumerable<string> strings);
        string GetPanelHead(string shipName);

        bool PromptUser(string promptMessage, out int value);
        bool PromptUser(string promptMessage, out string value);

        void OutputConditionAndWarnings(Ship ship, int shieldsDownLevel);
        void RenderSectors(SectorScanType scanType, ISubsystem subsystem);
        string Course();
    }
}
