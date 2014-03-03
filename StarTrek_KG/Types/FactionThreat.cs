namespace StarTrek_KG.Types
{
    public class FactionThreat
    {
        public string Threat { get; set; }
        public string Translation { get; set; }

        public FactionThreat(string threat, string translation)
        {
            this.Threat = threat;
            this.Translation = translation;
        }
    }
}
