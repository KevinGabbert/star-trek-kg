using System;
using System.Collections.Generic;

namespace StarTrek_KG
{
    public static class Constants
    {
        public const string EMPTY = "   ";
        public const string PLAYERSHIP = "<*>";
        public const string STARBASE = ">!<"; //∞
        public const string STAR = " * ";
        public const string DEBUG_MARKER = " X ";

        public const string NULL_MARKER = "!!"; //you shouldn't see this ever rendered.  if it is, then the game has an issue

        //--------------------------------------
        public const string ALLY = "<++>"; //todo: future feature

        public const string SCAN_SECTOR_DIVIDER = "│";

        //Make these part of the Map Object.  There should be only 1 map instance.. right?
        public static int SECTOR_MIN; 
        public static int SECTOR_MAX; 

        public static int Region_MIN; 
        public static int Region_MAX;

        public static int SHIELDS_DOWN_LEVEL;
        public static int LOW_ENERGY_LEVEL;

        public const int MOVEMENT_PRECISION = 10000;

        public static readonly List<String> MAP_DIRECTION = new List<String>() { "1", "2", "3", "4", "5", "6", "7", "8" };
        public static bool DEBUG_MODE;


        public static string SECTOR_INDICATOR = "§"; //todo: resource this out
    }
}
