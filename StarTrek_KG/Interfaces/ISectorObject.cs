using System;

namespace StarTrek_KG.Interfaces
{
    public interface ISectorObject
    {
        ISector Sector { get; } //todo: does this need to go away?
        
        //int Mass { get; } //useful for stars or black holes

        Type Type { get; set; }

        string Name { get; set; }
    }
}
