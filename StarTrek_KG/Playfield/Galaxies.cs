using System.Collections.Generic;
using System.Linq;

namespace StarTrek_KG.Playfield
{
    public class Galaxies : List<Galaxy>
    {
        public Galaxy this[string name] => this.FirstOrDefault(g => g != null && g.Name == name);
        public Galaxy GetById(int id) => this.FirstOrDefault(g => g != null && g.Id == id);
    }
}
