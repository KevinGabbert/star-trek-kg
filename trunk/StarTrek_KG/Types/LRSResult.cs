﻿namespace StarTrek_KG.Types
{
    public class LRSResult
    {
        private int _hostiles = -1;
        private int _starbases = -1;
        private int _stars = -1;

        public bool MyLocation { get; set; }
        public bool GalacticBarrier { get; set; }
        public bool Unknown { get; set; }

        public int Hostiles
        {
            get { return _hostiles; }
            set { _hostiles = value; }
        }

        public int Starbases
        {
            get { return _starbases; }
            set { _starbases = value; }
        }

        public int Stars
        {
            get { return _stars; }
            set { _stars = value; }
        }
    }
}
