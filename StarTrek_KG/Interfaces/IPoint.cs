namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// Implementers of this will be some type of coordinate object.
    /// </summary>
    public interface IPoint
    {
        /// <summary>
        /// Left (-) to Right (+)
        /// </summary>
        int X { get; set; }

        /// <summary>
        /// Up (-) to Down (+)
        /// </summary>
        int Y { get; set; }
    }
}
