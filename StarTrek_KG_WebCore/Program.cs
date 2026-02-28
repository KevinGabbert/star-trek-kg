using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using StarTrek_KG;
using StarTrek_KG.Config;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

EnsureConfigFile();

app.UseDefaultFiles();
app.UseStaticFiles();

var games = new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
var autoStartEnabled = IsAutoStartEnabled();

app.MapPost("/api/start", (StartRequest request) =>
{
    var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
        ? Guid.NewGuid().ToString("N")
        : request.SessionId;

    if (games.TryGetValue(sessionId, out var existing) && existing.Started)
    {
        return Results.Ok(new CommandResponse(sessionId, WrapResponseText("Game already running. Type 'stop' then 'start' again to start over.")));
    }

    var game = CreateGame();
    games[sessionId] = game;

    if (!game.Interact.OutputError)
    {
        game.RunSubscriber();
        var lines = WrapResponseText("Game Started..");
        lines.AddRange(game.Interact.Output.Queue.ToList());
        return Results.Ok(new CommandResponse(sessionId, lines));
    }

    return Results.Ok(new CommandResponse(sessionId, WrapResponseText("Game Initialization failed. Possible Configuration Error.")));
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
            return Results.Ok(StartSession(games, sessionId));

        case "end session":
        case "stop":
            return Results.Ok(StopSession(games, sessionId));

        case "clear session":
            return Results.Ok(ClearSession(games, sessionId));

        case "term menu":
            return Results.Ok(new CommandResponse(sessionId, WrapResponseList(new List<string>
            {
                " --- Terminal Menu ---",
                "start - starts a session",
                "end session - ends the currently running game",
                "reset config - reloads config",
                "release notes - see the latest release notes",
                "clear - clear the screen"
            })));

        case "release notes":
            return Results.Ok(new CommandResponse(sessionId, WrapResponseText("Under Construction")));
    }

    if (!games.TryGetValue(sessionId, out var game) || !game.Started)
    {
        var lines = WrapResponseText("Unrecognized Command. Game is not running. Type 'start' to start game");
        lines.Insert(1, "Err:");
        return Results.Ok(new CommandResponse(sessionId, lines));
    }

    if (game.GameOver)
    {
        return Results.Ok(new CommandResponse(sessionId, WrapResponseText("Game Over. Type 'start' to start again.")));
    }

    var turnResponse = game.SubscriberSendAndGetResponse(command);
    return Results.Ok(new CommandResponse(sessionId, WrapResponseList(turnResponse)));
});

app.MapPost("/api/prompt", (PromptRequest request) =>
{
    var sessionId = request.SessionId ?? string.Empty;

    if (!games.TryGetValue(sessionId, out var game) || !game.Started)
    {
        return Results.Ok(new PromptResponse(sessionId, "Terminal: "));
    }

    var prompt = game.Interact?.CurrentPrompt ?? "Terminal: ";
    return Results.Ok(new PromptResponse(sessionId, prompt));
});

app.MapGet("/api/settings", () =>
{
    return Results.Ok(new SettingsResponse(autoStartEnabled));
});

app.Run();

static void EnsureConfigFile()
{
    var configPath = System.IO.Path.Combine(AppContext.BaseDirectory, "app.config");
    if (!System.IO.File.Exists(configPath))
    {
        return;
    }

    AppContext.SetData("APP_CONFIG_FILE", configPath);
    ConfigurationManager.RefreshSection("StarTrekKGSettings");
    ConfigurationManager.RefreshSection("Commands");
}

static Game CreateGame()
{
    var settings = new StarTrekKGSettings();
    return new Game(settings);
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

static CommandResponse StartSession(ConcurrentDictionary<string, Game> games, string sessionId)
{
    var id = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId;
    var game = CreateGame();
    games[id] = game;
    game.RunSubscriber();
    var lines = WrapResponseText("Game Started..");
    lines.AddRange(game.Interact.Output.Queue.ToList());
    return new CommandResponse(id, lines);
}

static CommandResponse StopSession(ConcurrentDictionary<string, Game> games, string sessionId)
{
    if (!string.IsNullOrWhiteSpace(sessionId))
    {
        games.TryRemove(sessionId, out _);
    }
    return new CommandResponse(sessionId, WrapResponseText("-- Session Terminated --"));
}

static CommandResponse ClearSession(ConcurrentDictionary<string, Game> games, string sessionId)
{
    if (!string.IsNullOrWhiteSpace(sessionId))
    {
        games.TryRemove(sessionId, out _);
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

record StartRequest(string SessionId);
record CommandRequest(string SessionId, string Command);
record PromptRequest(string SessionId);
record CommandResponse(string SessionId, List<string> Lines);
record PromptResponse(string SessionId, string Prompt);
record SettingsResponse(bool AutoStart);
