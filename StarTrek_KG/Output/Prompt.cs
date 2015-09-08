using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Output
{
    public class PromptInfo
    {
        public PromptInfo(string defaultPrompt)
        {
            this.DefaultPrompt = defaultPrompt;
        }

        public SubsystemType SubSystem { get; set; }
        public string SubCommand { get; set; }
        public int Level { get; set; }
        public string DefaultPrompt { get; }
    }
}