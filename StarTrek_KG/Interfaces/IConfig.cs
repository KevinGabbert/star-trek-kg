using StarTrek_KG.Config;

namespace StarTrek_KG.Interfaces
{
    interface IConfig
    {
        StarTrekKGSettings Config { get; set; }
    }
}
