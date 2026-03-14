using System;
using System.Linq;
using System.Text;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Types;
using StarTrek_KG.Enums;
using StarTrek_KG.Subsystem;
using UnitTests.Output;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Commands;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Config;
using StarTrek_KG.Settings;

var game = new Game(new ConfigOverrideSettings(), new SetupOptions
{
    Initialize = true,
    StrictDeterministic = true,
    AddStars = false,
    AddNebulae = false,
    AddDeuterium = false,
    AddGraviticMines = false,
    CoordinateDefs = new CoordinateDefs
    {
        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip),
        new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,0)), CoordinateItem.HostileShip)
    }
});
var interact = (Interaction)game.Interact;
var oldFlagship = (Ship)game.Map.Playership;
var target = (Ship)game.Map.Sectors.GetActive().GetHostiles().First();
Shields.For(target).Energy = 49;
interact.ReadAndOutput(oldFlagship, game.Map.Text, "brd");
interact.Output.Clear();
interact.ReadAndOutput(oldFlagship, game.Map.Text, "tfg");
var render = new Render(game.Map.Game.Interact, game.Map.Game.Config);
var sector = game.Map.Sectors.GetActive();
var location = game.Map.Playership.GetLocation();
var sb = new StringBuilder();
game.Map.Game.Interact.Output.Clear();
render.CreateSRSViewScreen(sector, game.Map, location, sector.GetHostiles().Count, sector.Name, false, sb);
foreach (var line in game.Map.Game.Interact.Output.Queue) Console.WriteLine(line);
Console.WriteLine("Player row index=" + (1 + location.Coordinate.Y));
Console.WriteLine("Faction=" + target.Faction + " glyph='" + game.Config.Get.FactionDetails(target.Faction).designator + "' usePlayerGlyph=" + target.UsePlayerGlyph);

public sealed class ConfigOverrideSettings : IStarTrekKGSettings
{
    private readonly StarTrekKGSettings _inner = new StarTrekKGSettings();
    public StarTrekKGSettings Get { get => _inner.Get; set => _inner.Get = value; }
    public Names StarSystems => _inner.StarSystems;
    public NameValues ConsoleText => _inner.ConsoleText;
    public Factions Factions => _inner.Factions;
    public NameValues GameSettings => _inner.GameSettings;
    public MenusElement Menus => _inner.Menus;
    public System.Collections.Generic.List<CommandDef> LoadCommands() => _inner.LoadCommands();
    public StarTrekKGSettings GetConfig() => _inner.GetConfig();
    public System.Collections.Generic.List<string> ShipNames(StarTrek_KG.TypeSafeEnums.FactionName faction) => _inner.ShipNames(faction);
    public System.Collections.Generic.List<FactionThreat> GetThreats(StarTrek_KG.TypeSafeEnums.FactionName faction) => _inner.GetThreats(faction);
    public MenuItems GetMenuItems(string menuName) => _inner.GetMenuItems(menuName);
    public System.Collections.Generic.List<string> GetStarSystems() => _inner.GetStarSystems();
    public string GetText(string name) => _inner.GetText(name);
    public string GetText(string t1, string t2) => _inner.GetText(t1, t2);
    public T GetSetting<T>(string name)
    {
        if (string.Equals(name, "BoardingSuccessMinRoll", StringComparison.OrdinalIgnoreCase))
            return (T)Convert.ChangeType("1", typeof(T));
        return _inner.GetSetting<T>(name);
    }
    public string Setting(string name) => this.GetSetting<string>(name);
    public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false) => _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
    public void Reset() => _inner.Reset();
}
