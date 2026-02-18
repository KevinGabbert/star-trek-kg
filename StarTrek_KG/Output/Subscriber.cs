using StarTrek_KG.Interfaces;
using System;

namespace StarTrek_KG.Output
{
    public class Subscriber
    {
        IStarTrekKGSettings Config { get; }
        public PromptInfo PromptInfo { get; }

        public Subscriber(IStarTrekKGSettings config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config), "Config cannot be null.");

            this.Config = config;

            string defaultPrompt = this.Config.GetText("defaultPrompt");
            PromptInfo = new PromptInfo(defaultPrompt);
        }
    }
}