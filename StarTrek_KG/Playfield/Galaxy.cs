using System;

namespace StarTrek_KG.Playfield
{
    public class Galaxy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Sectors Sectors { get; set; }

        public Galaxy()
        {
            this.Name = string.Empty;
            this.Sectors = null;
        }

        public Galaxy(int id, string name, Sectors sectors)
        {
            this.Id = id;
            this.Name = name ?? string.Empty;
            this.Sectors = sectors ?? throw new ArgumentNullException(nameof(sectors));
        }
    }
}
