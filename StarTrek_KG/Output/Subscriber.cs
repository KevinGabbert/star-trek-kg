using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Output
{
    public class Subscriber
    {
        IStarTrekKGSettings Config { get; }
        public PromptInfo PromptInfo { get; }

        public Subscriber(IStarTrekKGSettings config)
        {
            this.Config = config;

            string defaultPrompt = this.Config.GetText("defaultPrompt");
            PromptInfo = new PromptInfo(defaultPrompt);

            bool enabled = this.Config.GetSetting<bool>("IsSubscriberApp");
            this.Enabled = enabled;
        }

        public bool Enabled { get; set; }
    }
}