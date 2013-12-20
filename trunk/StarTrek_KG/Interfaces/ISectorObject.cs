using System;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface ISectorObject
    {
        Sector Sector { get; } //todo: does this need to go away?
        
        //int Mass { get; } //useful for stars or black holes

        Type Type { get; set; }

    }
}
