using System;

namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// Object found in a sector.  One object per sector
    /// </summary>
    public interface ISectorObject
    {
        ISector Sector { get; set; }
        Type Type { get; set; }
        string Name { get; set; }

        //int Mass { get; } //todo: future feature. useful for stars or black holes
    }
}
