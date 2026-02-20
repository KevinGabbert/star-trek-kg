using StarTrek_KG.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarTrek_KG.Commands
{
    public class CommandDispatcher
    {
        private readonly CommandRepository _repository;
        private readonly Dictionary<string, Action<IShip, string>> _handlers;
        private readonly IInteraction _interaction;

        public CommandDispatcher(IInteraction interaction, CommandRepository repository)
        {
            _interaction = interaction;
            _repository = repository;
        }

        public CommandDispatcher(CommandRepository repository)
        {
            _repository = repository;
            _handlers = new Dictionary<string, Action<IShip, string>>();
        }

        public void RegisterHandler(string commandKey, string subcommandKey, Action<IShip, string> handler)
        {
            _handlers[$"{commandKey}.{subcommandKey}"] = handler;
        }

        public void HandleInput(string userInput, IShip ship)
        {
            // Split top-level command from subcommand
            string[] parts = userInput
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .ToArray();

            string commandKey = parts[0];

            CommandDef command = _repository.GetCommand(commandKey);
            if (command == null)
            {
                ship.OutputLine($"Unknown command: {commandKey}");
                return;
            }

            if (command.Subcommands.Count > 0 && parts.Length == 1)
            {
                // User just typed top-level command
                ship.OutputLine($"Available {commandKey} subcommands:");
                foreach (SubcommandDef sc in command.Subcommands)
                {
                    ship.OutputLine($"  {sc.Key} - {sc.Description}");
                }
                return;
            }

            // User typed command and subcommand
            var subKey = parts.Length > 1 ? parts[1] : "";
            var sub = command.Subcommands.FirstOrDefault(s => s.Key.Equals(subKey, StringComparison.OrdinalIgnoreCase));
            if (sub == null)
            {
                ship.OutputLine($"Unknown subcommand: {subKey}");
                return;
            }

            // Prompt if needed
            // Prompt if needed
            string value = "";
            if (!string.IsNullOrWhiteSpace(sub.Prompt))
            {
                // Send prompt text to output queue
                ship.OutputLine(sub.Prompt);

                // Trigger interaction system's subscriber prompt
                _interaction.PromptUserSubscriber(sub.Prompt, out value);
            }

            string handlerKey = $"{commandKey}.{subKey}";
            if (_handlers.TryGetValue(handlerKey, out var handler))
            {
                handler(ship, value);
            }
            else
            {
                ship.OutputLine($"No handler registered for {handlerKey}");
            }
        }
    }

}
