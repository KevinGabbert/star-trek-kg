using System.Collections.Generic;
using System.Linq;

namespace StarTrek_KG.Playfield
{
    public class Points : List<Point>
    {
        public Point Get(int qX, int qY)
        {
            return this.Single(s => s.X == qX && s.Y == qY);
        }
    }
}
