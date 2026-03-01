using System;
using System.Collections.Generic;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Commands
{
    public static class WarGamesStartup
    {
        private const string DefaultPostStartCommand = "srs";
        private const string SettingName = "war-games-start-command";

        public static string ResolvePostStartCommand(IStarTrekKGSettings settings)
        {
            if (settings == null)
            {
                return DefaultPostStartCommand;
            }

            try
            {
                var configured = settings.GetSetting<string>(SettingName);
                if (string.IsNullOrWhiteSpace(configured))
                {
                    return DefaultPostStartCommand;
                }

                return configured.Trim().ToLowerInvariant();
            }
            catch
            {
                return DefaultPostStartCommand;
            }
        }

        public static List<string> ExecuteConfiguredPostStartCommand(Game game, IStarTrekKGSettings settings)
        {
            if (game == null)
            {
                return new List<string>();
            }

            var command = ResolvePostStartCommand(settings);
            if (string.IsNullOrWhiteSpace(command))
            {
                return new List<string>();
            }

            var response = game.SubscriberSendAndGetResponse(command);
            return response ?? new List<string>();
        }
    }
}
