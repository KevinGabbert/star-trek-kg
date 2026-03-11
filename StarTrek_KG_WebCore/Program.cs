using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using StarTrek_KG;
using StarTrek_KG.Commands;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

EnsureConfigFile();

app.UseDefaultFiles();
app.UseStaticFiles();

var sessions = new ConcurrentDictionary<string, SessionState>(StringComparer.OrdinalIgnoreCase);
var autoStartEnabled = IsAutoStartEnabled();
var autoStartMode = GetAutoStartMode();

app.MapPost("/api/start", (StartRequest request) =>
{
    var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
        ? Guid.NewGuid().ToString("N")
        : request.SessionId;

    return Results.Ok(StartSession(sessions, sessionId, SessionMode.Game));
});

app.MapPost("/api/command", (CommandRequest request) =>
{
    var command = request.Command?.Trim() ?? string.Empty;
    var sessionId = request.SessionId ?? string.Empty;

    if (string.IsNullOrWhiteSpace(command))
    {
        return Results.Ok(new CommandResponse(sessionId, WrapResponseText("No command entered.")));
    }

    switch (command.ToLowerInvariant())
    {
        case "start":
            return Results.Ok(StartSession(sessions, sessionId, SessionMode.Game));

        case "war games":
            return Results.Ok(StartSession(sessions, sessionId, SessionMode.WarGames));
        case "systems cascade":
            return Results.Ok(StartSession(sessions, sessionId, SessionMode.SystemsCascade));

        case "end session":
        case "stop":
            return Results.Ok(StopSession(sessions, sessionId));

        case "clear session":
            return Results.Ok(ClearSession(sessions, sessionId));

        case "term menu":
            return Results.Ok(new CommandResponse(sessionId, WrapResponseList(new List<string>
            {
                " --- Terminal Menu ---",
                "start - starts a normal game session",
                "war games - starts a deterministic scenario session",
                "systems cascade - starts systems failure survival mode",
                "end session - ends the currently running game",
                "reset config - reloads config",
                "release notes - see the latest release notes",
                "clear - clear the screen"
            })));

        case "release notes":
            return Results.Ok(new CommandResponse(sessionId, WrapResponseText("Under Construction")));
    }

    if (!sessions.TryGetValue(sessionId, out var state) || !state.Game.Started)
    {
        var lines = WrapResponseText("Unrecognized Command. Game is not running. Type 'start', 'war games', or 'systems cascade' to start game");
        lines.Insert(1, "Err:");
        return Results.Ok(new CommandResponse(sessionId, lines));
    }

    if (state.Game.GameOver)
    {
        return Results.Ok(new CommandResponse(sessionId, WrapResponseText("Game Over. Type 'start', 'war games', or 'systems cascade' to start again.")));
    }

    var turnResponse = state.Game.SubscriberSendAndGetResponse(command);
    return Results.Ok(new CommandResponse(sessionId, WrapResponseList(turnResponse)));
});

app.MapPost("/api/prompt", (PromptRequest request) =>
{
    var sessionId = request.SessionId ?? string.Empty;

    if (!sessions.TryGetValue(sessionId, out var state) || !state.Game.Started)
    {
        return Results.Ok(new PromptResponse(sessionId, "Terminal: "));
    }

    var prompt = state.Game.Interact?.CurrentPrompt ?? "Terminal: ";
    return Results.Ok(new PromptResponse(sessionId, prompt));
});

app.MapGet("/api/settings", () =>
{
    return Results.Ok(new SettingsResponse(autoStartEnabled, autoStartMode.ToModeString()));
});

app.Run();

static void EnsureConfigFile()
{
    var configPath = Path.Combine(AppContext.BaseDirectory, "app.config");
    if (!File.Exists(configPath))
    {
        return;
    }

    AppContext.SetData("APP_CONFIG_FILE", configPath);
    ConfigurationManager.RefreshSection("StarTrekKGSettings");
    ConfigurationManager.RefreshSection("Commands");
}

static CommandResponse StartSession(ConcurrentDictionary<string, SessionState> sessions, string sessionId, SessionMode requestedMode)
{
    var id = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId;

    if (sessions.TryGetValue(id, out var existing) && existing.Game.Started)
    {
        if (existing.Mode != requestedMode)
        {
            return new CommandResponse(id, WrapResponseText($"Session is already running in '{existing.Mode.ToModeString()}' mode. Type 'stop' and then start the desired mode."));
        }

        return new CommandResponse(id, WrapResponseText("Game already running. Type 'stop' then start again to start over."));
    }

    var created = CreateGameForMode(requestedMode);
    if (created.errorMessage != null)
    {
        return new CommandResponse(id, WrapResponseText(created.errorMessage));
    }

    var state = new SessionState(created.game, requestedMode);
    sessions[id] = state;
    state.Game.RunSubscriber();

    var lines = WrapResponseText(requestedMode switch
    {
        SessionMode.WarGames => "War Games Session Started..",
        SessionMode.SystemsCascade => "Systems Cascade Session Started..",
        _ => "Game Started.."
    });
    lines.AddRange(state.Game.Interact.Output.Queue.ToList());

    if (requestedMode == SessionMode.WarGames)
    {
        var postStartLines = WarGamesStartup.ExecuteConfiguredPostStartCommand(state.Game, state.Game.Config);
        lines.AddRange(postStartLines);
    }
    else if (requestedMode == SessionMode.SystemsCascade)
    {
        var postStartLines = state.Game.SubscriberSendAndGetResponse("srs");
        lines.AddRange(postStartLines);
    }

    return new CommandResponse(id, lines);
}

static (Game game, string errorMessage) CreateGameForMode(SessionMode mode)
{
    var settings = new StarTrekKGSettings();

    if (mode == SessionMode.Game)
    {
        return (new Game(settings), null);
    }

    if (mode == SessionMode.WarGames)
    {
        try
        {
            var setup = LoadWarGamesSetup(settings);
            return (new Game(settings, setup), null);
        }
        catch (Exception ex)
        {
            return (null, $"War games initialization failed: {ex.Message}");
        }
    }

    try
    {
        var setup = SystemsCascadeSetupLoader.Load(settings);
        return (new Game(settings, setup), null);
    }
    catch (Exception ex)
    {
        return (null, $"Systems Cascade initialization failed: {ex.Message}");
    }
}

static SetupOptions LoadWarGamesSetup(StarTrekKGSettings settings)
{
    var scenarioPath = Path.Combine(AppContext.BaseDirectory, "war-games.config.json");
    if (!File.Exists(scenarioPath))
    {
        throw new InvalidOperationException($"Missing scenario file '{scenarioPath}'.");
    }

    var json = File.ReadAllText(scenarioPath);
    var scenario = JsonSerializer.Deserialize<WarGamesScenario>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (scenario == null)
    {
        throw new InvalidOperationException("Scenario config is empty or invalid JSON.");
    }

    if (scenario.Player?.Sector == null || scenario.Player.Coordinate == null)
    {
        throw new InvalidOperationException("Scenario must define player.sector and player.coordinate.");
    }

    var coordinateMin = settings.GetSetting<int>("COORDINATE_MIN");
    var coordinateMaxExclusive = settings.GetSetting<int>("COORDINATE_MAX");
    var sectorMin = settings.GetSetting<int>("SECTOR_MIN");
    var sectorMaxExclusive = settings.GetSetting<int>("SECTOR_MAX");

    var strict = scenario.StrictDeterministic ?? true;
    var features = scenario.Features ?? new WarGamesFeatures();

    var defs = new CoordinateDefs();
    var occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    AddDef(defs, occupied, CoordinateItem.PlayerShip, scenario.Player.Sector, scenario.Player.Coordinate, coordinateMin, coordinateMaxExclusive, sectorMin, sectorMaxExclusive);

    foreach (var actor in scenario.Actors ?? new List<WarGamesActor>())
    {
        if (actor?.Sector == null || actor.Coordinate == null)
        {
            throw new InvalidOperationException("Each actor requires sector and coordinate.");
        }

        var item = ParseActorType(actor.Type);
        AddDef(defs, occupied, item, actor.Sector, actor.Coordinate, coordinateMin, coordinateMaxExclusive, sectorMin, sectorMaxExclusive);
    }

    return new SetupOptions
    {
        Initialize = true,
        IsWarGamesMode = true,
        StrictDeterministic = strict,
        OpeningPictureKey = "D-4",
        AddStars = features.AddStars ?? false,
        AddNebulae = features.AddNebulae ?? false,
        AddDeuterium = features.AddDeuterium ?? false,
        AddGraviticMines = features.AddGraviticMines ?? false,
        CoordinateDefs = defs
    };
}

static void AddDef(
    CoordinateDefs defs,
    HashSet<string> occupied,
    CoordinateItem item,
    WarGamesPoint sector,
    WarGamesPoint coordinate,
    int coordinateMin,
    int coordinateMaxExclusive,
    int sectorMin,
    int sectorMaxExclusive)
{
    ValidateRange("sector.x", sector.X, sectorMin, sectorMaxExclusive);
    ValidateRange("sector.y", sector.Y, sectorMin, sectorMaxExclusive);
    ValidateRange("coordinate.x", coordinate.X, coordinateMin, coordinateMaxExclusive);
    ValidateRange("coordinate.y", coordinate.Y, coordinateMin, coordinateMaxExclusive);

    var key = $"{sector.X}:{sector.Y}:{coordinate.X}:{coordinate.Y}";
    if (!occupied.Add(key))
    {
        throw new InvalidOperationException($"Duplicate actor location at sector [{sector.X},{sector.Y}] coordinate [{coordinate.X},{coordinate.Y}].");
    }

    var location = new LocationDef(new Point(sector.X, sector.Y), new Point(coordinate.X, coordinate.Y));
    var def = new CoordinateDef(item)
    {
        SectorDef = new Point(sector.X, sector.Y),
        Coordinate = new Coordinate(location),
        Item = item
    };

    defs.Add(def);
}

static CoordinateItem ParseActorType(string type)
{
    if (string.IsNullOrWhiteSpace(type))
    {
        throw new InvalidOperationException("Actor type cannot be empty.");
    }

    switch (type.Trim().ToLowerInvariant())
    {
        case "hostileship":
        case "hostile":
            return CoordinateItem.HostileShip;
        case "star":
            return CoordinateItem.Star;
        case "starbase":
            return CoordinateItem.Starbase;
        case "deuterium":
            return CoordinateItem.Deuterium;
        case "graviticmine":
        case "gravitic-mine":
            return CoordinateItem.GraviticMine;
        default:
            throw new InvalidOperationException($"Unsupported actor type '{type}'.");
    }
}

static void ValidateRange(string label, int value, int minInclusive, int maxExclusive)
{
    if (value < minInclusive || value >= maxExclusive)
    {
        throw new InvalidOperationException($"{label} must be in range [{minInclusive}..{maxExclusive - 1}].");
    }
}

static bool IsAutoStartEnabled()
{
    try
    {
        var settings = new StarTrekKGSettings();
        return settings.GetSetting<bool>("auto-start");
    }
    catch
    {
        return false;
    }
}

static SessionMode GetAutoStartMode()
{
    try
    {
        var settings = new StarTrekKGSettings();
        var mode = settings.GetSetting<string>("auto-start-mode");
        return ParseMode(mode);
    }
    catch
    {
        return SessionMode.Game;
    }
}

static SessionMode ParseMode(string mode)
{
    if (string.Equals(mode, "war games", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(mode, "war-games", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(mode, "wargames", StringComparison.OrdinalIgnoreCase))
    {
        return SessionMode.WarGames;
    }

    if (string.Equals(mode, "systems cascade", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(mode, "systems-cascade", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(mode, "systemscascade", StringComparison.OrdinalIgnoreCase))
    {
        return SessionMode.SystemsCascade;
    }

    return SessionMode.Game;
}

static CommandResponse StopSession(ConcurrentDictionary<string, SessionState> sessions, string sessionId)
{
    if (!string.IsNullOrWhiteSpace(sessionId))
    {
        sessions.TryRemove(sessionId, out _);
    }
    return new CommandResponse(sessionId, WrapResponseText("-- Session Terminated --"));
}

static CommandResponse ClearSession(ConcurrentDictionary<string, SessionState> sessions, string sessionId)
{
    if (!string.IsNullOrWhiteSpace(sessionId))
    {
        sessions.TryRemove(sessionId, out _);
    }
    return new CommandResponse(sessionId, WrapResponseText("Session Cleared.."));
}

static List<string> WrapResponseText(string text)
{
    return new List<string> { Environment.NewLine, text, Environment.NewLine };
}

static List<string> WrapResponseList(IList<string> text)
{
    if (text == null)
    {
        return new List<string>();
    }

    var list = new List<string>(text);
    list.Insert(0, Environment.NewLine);
    list.Add(Environment.NewLine);
    return list;
}

enum SessionMode
{
    Game,
    WarGames,
    SystemsCascade
}

static class SessionModeExtensions
{
    public static string ToModeString(this SessionMode mode)
    {
        return mode switch
        {
            SessionMode.WarGames => "war games",
            SessionMode.SystemsCascade => "systems cascade",
            _ => "game"
        };
    }
}

sealed record SessionState(Game Game, SessionMode Mode);

sealed class WarGamesScenario
{
    public string ScenarioName { get; set; }
    public bool? StrictDeterministic { get; set; }
    public WarGamesFeatures Features { get; set; }
    public WarGamesPlayer Player { get; set; }
    public List<WarGamesActor> Actors { get; set; }
}

sealed class WarGamesFeatures
{
    public bool? AddStars { get; set; }
    public bool? AddNebulae { get; set; }
    public bool? AddDeuterium { get; set; }
    public bool? AddGraviticMines { get; set; }
}

sealed class WarGamesPlayer
{
    public string Faction { get; set; }
    public string Name { get; set; }
    public WarGamesPoint Sector { get; set; }
    public WarGamesPoint Coordinate { get; set; }
}

sealed class WarGamesActor
{
    public string Type { get; set; }
    public WarGamesPoint Sector { get; set; }
    public WarGamesPoint Coordinate { get; set; }
}

sealed class WarGamesPoint
{
    public int X { get; set; }
    public int Y { get; set; }
}

record StartRequest(string SessionId);
record CommandRequest(string SessionId, string Command);
record PromptRequest(string SessionId);
record CommandResponse(string SessionId, List<string> Lines);
record PromptResponse(string SessionId, string Prompt);
record SettingsResponse(bool AutoStart, string AutoStartMode);

static class SystemsCascadeSetupLoader
{
    public static SetupOptions Load(StarTrekKGSettings settings)
    {
        var scenarioPath = Path.Combine(AppContext.BaseDirectory, "systems-cascade.config.json");
        if (!File.Exists(scenarioPath))
        {
            throw new InvalidOperationException($"Missing scenario file '{scenarioPath}'.");
        }

        var json = File.ReadAllText(scenarioPath);
        var scenario = JsonSerializer.Deserialize<SystemsCascadeScenario>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (scenario == null)
        {
            throw new InvalidOperationException("Systems cascade config is empty or invalid JSON.");
        }

        var destinationDistance = scenario.DestinationDistance ?? 5;
        if (destinationDistance < 1)
        {
            destinationDistance = 1;
        }

        var anomalyDensity = scenario.AnomalyDensityPercent ?? 10;
        if (anomalyDensity < 1)
        {
            anomalyDensity = 1;
        }
        if (anomalyDensity > 100)
        {
            anomalyDensity = 100;
        }

        var playerLocation = scenario.PlayerStart ?? new WarGamesPoint { X = 0, Y = 0 };
        var playerSector = scenario.PlayerSector ?? new WarGamesPoint { X = 0, Y = 0 };

        var defs = new CoordinateDefs
        {
            new CoordinateDef(new LocationDef(new Point(playerSector.X, playerSector.Y), new Point(playerLocation.X, playerLocation.Y)), CoordinateItem.PlayerShip)
        };

        var starbaseCount = 12;
        try
        {
            starbaseCount = settings.GetSetting<int>("starbases");
        }
        catch
        {
        }

        if (starbaseCount < 1)
        {
            starbaseCount = 1;
        }

        for (var i = 0; i < starbaseCount; i++)
        {
            defs.Add(new CoordinateDef(CoordinateItem.Starbase));
        }

        return new SetupOptions
        {
            Initialize = true,
            IsSystemsCascadeMode = true,
            StrictDeterministic = true,
            OpeningPictureKey = "D-4",
            AddStars = scenario.AddStars ?? true,
            AddNebulae = scenario.AddNebulae ?? true,
            AddDeuterium = true,
            AddGraviticMines = false,
            AddEnergyAnomalies = true,
            SystemsCascadeAnomalyDensityPercent = anomalyDensity,
            SystemsCascadeDestinationDistance = destinationDistance,
            SystemsCascadeEmergencyPowerTurns = scenario.EmergencyPowerTurns ?? 6,
            SystemsCascadeEmergencyPowerPerTurn = scenario.EmergencyPowerPerTurn ?? 120,
            SystemsCascadeNebulaDeuteriumMultiplier = scenario.NebulaDeuteriumMultiplier ?? 3,
            SystemsCascadeEscalationChancePercent = scenario.EscalationChancePercent ?? 25,
            SystemsCascadeGraceTurns = scenario.GraceTurns ?? 3,
            SystemsCascadeAnomalyEnergyHit = scenario.AnomalyEnergyHit ?? 60,
            SystemsCascadeAnomalyScannerNoisePerHit = scenario.AnomalyScannerNoisePerHit ?? 2,
            SystemsCascadeAnomalyLrsNoisePerHit = scenario.AnomalyLrsNoisePerHit ?? 20,
            SystemsCascadeIntroLines = scenario.IntroLines ?? new List<string>(),
            SystemsCascadePowerHelpText = scenario.PowerHelpText,
            CoordinateDefs = defs
        };
    }
}

sealed class SystemsCascadeScenario
{
    public int? DestinationDistance { get; set; }
    public int? AnomalyDensityPercent { get; set; }
    public int? EmergencyPowerTurns { get; set; }
    public int? EmergencyPowerPerTurn { get; set; }
    public int? NebulaDeuteriumMultiplier { get; set; }
    public int? EscalationChancePercent { get; set; }
    public int? GraceTurns { get; set; }
    public int? AnomalyEnergyHit { get; set; }
    public int? AnomalyScannerNoisePerHit { get; set; }
    public int? AnomalyLrsNoisePerHit { get; set; }
    public bool? AddNebulae { get; set; }
    public bool? AddStars { get; set; }
    public WarGamesPoint PlayerSector { get; set; }
    public WarGamesPoint PlayerStart { get; set; }
    public List<string> IntroLines { get; set; }
    public string PowerHelpText { get; set; }
}
