using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class Menu
    {
        public string Description { get; }
        public string Name { get; }

        private readonly int value;

        //todo: resource out menu - Delete this construct
        //todo: pull these values from Config file
        public static readonly Menu nav = new Menu(1, "nav", "");
        public static readonly Menu wrp = new Menu(2, "wrp", "Warp Navigation");
        public static readonly Menu irs = new Menu(3, "irs", "Immediate Range Scan");
        public static readonly Menu srs = new Menu(4, "srs", "Short Range Scan");
        public static readonly Menu lrs = new Menu(5, "lrs", "Long Range Scan");
        public static readonly Menu crs = new Menu(6, "crs", "Combined Range Scan");
        public static readonly Menu pha = new Menu(7, "pha", "Phaser Control");                                    
        public static readonly Menu tor = new Menu(8, "tor", "Photon Torpedo Control");
        public static readonly Menu she = new Menu(9, "she", "Shield Control");
        public static readonly Menu com = new Menu(10, "com", "Access Computer");
        public static readonly Menu imp = new Menu(11, "imp", "Impulse Navigation");
        public static readonly Menu nto = new Menu(12, "nto", "Navigate To Object");
        public static readonly Menu toq = new Menu(13, "toq", "Target Object in this Sector");
        public static readonly Menu dmg = new Menu(14, "dmg", "Damage Control");              
        public static readonly Menu dbg = new Menu(15, "dbg", "Debug Test Mode");

        //todo: support these commands
        public static readonly Menu ver = new Menu(16, "ver", "");
        public static readonly Menu clear = new Menu(17, "clear", "");

        private static Dictionary<string, Menu> instance = new Dictionary<string, Menu>();

        private Menu(int value, string name, string description)
        {
            this.Name = name;
            this.value = value;
            this.Description = description;

            if (instance == null)
            {
                instance = new Dictionary<string, Menu>();
                // instance.Add(name, null);
            }

            instance[name] = this;
        }

        public override string ToString()
        {
            return Name;
        }

        public static explicit operator Menu(string str)
        {
            Menu result;
            if (instance.TryGetValue(str, out result))
                return result;
            else
                throw new InvalidCastException();
        }
    }
}
