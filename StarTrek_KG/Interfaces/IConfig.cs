namespace StarTrek_KG.Interfaces
{
    ///<summary>
    /// Implementers of this will be using configuration settings
    ///</summary>
    public interface IConfig
    {
        IStarTrekKGSettings Config { get; set; }
    }
}
