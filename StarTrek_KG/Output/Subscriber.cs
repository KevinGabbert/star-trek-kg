namespace StarTrek_KG.Output
{
    public class Subscriber
    {
        public PromptInfo PromptInfo { get; }

        public Subscriber()
        {
            PromptInfo = new PromptInfo();
        }

        public Subscriber(bool enabled)
        {
            PromptInfo = new PromptInfo();
            this.Enabled = enabled;
        }

        public bool Enabled { get; set; }
    }
}