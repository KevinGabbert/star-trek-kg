namespace StarTrek_KG.Constants.Commands
{
    public static class Commands
    {
        //TODO: eventually make this all data driven from the config file.  (Like done in Shields subsystem)

        public static class Navigation
        {
            public const string Warp = "wrp";
            public const string Impulse = "imp";
            public const string NavigateToObject = "nto";
        }

        public static class Computer
        {
            //todo: break these out into subsystems of computer

            public const string GalacticRecord = "rec";
            public const string Status = "sta";

            public const string TorpedoCalculator = "tor";
            public const string TargetObjectInRegion = "toq";

            public const string StarbaseCalculator = "bas";
            public const string NavigationCalculator = "wrp";
            public const string TranslateLastMessage = "tlm";
        }

        public static class DamageControl
        {
            public const string FixSubsystem = "fix";
        }
    }
}
