using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Output
{
    public class PromptInfo
    {
        public PromptInfo()
        {
        }

        public SubsystemType SubSystem { get; set; }
        public string SubCommand { get; set; }
        public int Level { get; set; }
    }
}