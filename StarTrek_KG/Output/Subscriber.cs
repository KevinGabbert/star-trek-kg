using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Output
{
    public class Subscriber
    {
        public Subscriber()
        {
        }

        public Subscriber(bool enabled)
        {
            this.Enabled = enabled;
        }

        public bool Enabled { get; set; }

        public SubsystemType PromptSubSystem { get; set; }
        public string PromptSubCommand { get; set; }
        public int PromptLevel { get; set; }
    }
}