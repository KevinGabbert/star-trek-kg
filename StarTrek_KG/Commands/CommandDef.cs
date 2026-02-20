using System.Collections.Generic;

namespace StarTrek_KG.Commands
{
    public class CommandDef
    {
        public string Key { get; set; }
        public string Subsystem { get; set; }
        public string Description { get; set; }
        public List<SubcommandDef> Subcommands { get; set; } = new List<SubcommandDef>();
    }
}
