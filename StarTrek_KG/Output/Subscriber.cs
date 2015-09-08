namespace StarTrek_KG.Output
{
    public class Subscriber
    {
        public Prompt Prompt { get; }

        public Subscriber()
        {
            Prompt = new Prompt();
        }

        public Subscriber(bool enabled)
        {
            Prompt = new Prompt();
            this.Enabled = enabled;
        }

        public bool Enabled { get; set; }
    }
}