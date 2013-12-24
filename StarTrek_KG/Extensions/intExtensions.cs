namespace StarTrek_KG.Extensions
{
    public static class intExtensions
    {
        public static string FormatForLRS(this int count)
        {
            return count > 15 ? "*" : count.ToString("X");
        }
    }
}
