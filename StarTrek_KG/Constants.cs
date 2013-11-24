﻿using System;
using System.Collections.Generic;

namespace StarTrek_KG
{
    public class Constants
    {
        public const string EMPTY = "   ";
        public const string ENTERPRISE = "<*>";
        public const string HOSTILE = "+++";
        public const string STARBASE = ">!<";
        public const string STAR = " * ";
        public const string ALLY = "<++>";

        public static int SECTOR_MIN; 
        public static int SECTOR_MAX; 

        public static int QUADRANT_MIN; 
        public static int QUADRANT_MAX;

        public static int SHIELDS_DOWN_LEVEL;
        public static int LOW_ENERGY_LEVEL;

        public const int MOVEMENT_PRECISION = 1000;

        public static List<String> MAP_DIRECTION = new List<String>(){ "n", "ne", "e", "se", "s", "sw", "w", "nw" };
    }
}
