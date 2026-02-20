using StarTrek_KG.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarTrek_KG.Commands
{
    public class CommandRepository
    {
        private readonly List<CommandDef> _commands;

        public CommandRepository(IStarTrekKGSettings settings)
        {
            _commands = settings.LoadCommands();
        }

        public CommandDef GetCommand(string key)
        {
            return _commands.FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }

}
